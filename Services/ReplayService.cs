using MarbleServer.Configuration;
using MarbleServer.Data;
using MarbleServer.Exceptions;
using MarbleServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MarbleServer.Services
{
    public class ReplayService
    {
        private readonly MarbleDbContext _db;
        private readonly ReplaySettings _settings;

        public ReplayService(
            MarbleDbContext db,
            IOptions<ReplaySettings> options)
        {
            _db = db;
            _settings = options.Value;
        }

        public async Task<int> UploadReplayAsync(
            int playerId,
            int scoreId,
            int timeMs,
            IFormFile replay)
        {
            long maxBytes =
                _settings.MaxFileSizeMB * 1024L * 1024L;

            if (replay.Length > maxBytes)
            {
                throw new ValidationException(
                    "Replay file is too large.");
            }

            Score? score = await _db.Scores
                .Include(s => s.Replay)
                .FirstOrDefaultAsync(s => s.Id == scoreId);

            if (score == null)
            {
                throw new NotFoundException(
                    "Score",
                    scoreId);
            }

            if (score.PlayerId != playerId)
            {
                throw new ForbiddenException(
                    "You do not own this score.");
            }

            if (score.TimeMs != timeMs)
            {
                throw new ConflictException(
                    "This replay no longer matches the score.");
            }

            int bestTime = await _db.Scores
                .Where(s => s.Level == score.Level)
                .MinAsync(s => s.TimeMs);

            if (score.TimeMs != bestTime)
            {
                throw new ConflictException(
                    "This score is no longer the World Record.");
            }

            if (score.Replay != null)
            {
                string oldFile = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    _settings.Folder,
                    score.Replay.FileName);

                if (File.Exists(oldFile))
                {
                    File.Delete(oldFile);
                }

                _db.Replays.Remove(score.Replay);
            }

            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                _settings.Folder);

            Directory.CreateDirectory(uploadsFolder);

            string extension =
                Path.GetExtension(replay.FileName);

            string fileName =
                $"{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine(
                uploadsFolder,
                fileName);

            await using (FileStream stream =
                new FileStream(filePath, FileMode.Create))
            {
                await replay.CopyToAsync(stream);
            }

            Replay replayEntity = new Replay
            {
                ScoreId = scoreId,
                FileName = fileName,
                FileSize = replay.Length,
                UploadedAt = DateTime.UtcNow
            };

            _db.Replays.Add(replayEntity);

            await _db.SaveChangesAsync();

            return replayEntity.Id;
        }

        public async Task<(byte[] Data, string FileName)> DownloadReplayAsync(
            int scoreId)
        {
            Replay? replay = await _db.Replays
                .FirstOrDefaultAsync(r => r.ScoreId == scoreId);

            if (replay == null)
            {
                throw new NotFoundException(
                    "Replay",
                    scoreId);
            }

            string filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                _settings.Folder,
                replay.FileName);

            if (!File.Exists(filePath))
            {
                throw new NotFoundException(
                    $"Replay file '{replay.FileName}' was not found on disk.");
            }

            return
            (
                await File.ReadAllBytesAsync(filePath),
                replay.FileName
            );
        }
    }
}