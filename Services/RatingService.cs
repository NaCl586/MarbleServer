using MarbleServer.Models.Leaderboard;

namespace MarbleServer.Services
{
    public class RatingService
    {
        private const double MaxTimeSeconds =
            5999.999;

        private readonly RatingReferenceData _referenceData;

        public RatingService(
            RatingReferenceData referenceData)
        {
            _referenceData = referenceData;
        }

        public bool TryGetMission(
            string level,
            out RatingMissionData? data)
        {
            return _referenceData.TryGetMission(
                level,
                out data);
        }

        public int CalculateRating(
            string level,
            int timeMs)
        {
            if (string.IsNullOrWhiteSpace(level))
                return 0;

            if (timeMs <= 0)
                return 0;

            if (!_referenceData.TryGetMission(
                    level,
                    out RatingMissionData? data))
            {
                return 0;
            }

            return CalculateNullRating(
                timeMs,
                data!.RatingInfo);
        }

        private static int CalculateNullRating(
            int scoreMs,
            MissionRatingInfo ratingInfo)
        {
            if (ratingInfo.Disabled == true)
                return 0;

            double timeOffset =
                ratingInfo.TimeOffset ?? 0;

            if (scoreMs < timeOffset)
                return -2;

            double parTime =
                ratingInfo.ParTime ?? 0;

            double platinumTime =
                ratingInfo.PlatinumTime ?? 0;

            double ultimateTime =
                ratingInfo.UltimateTime ?? 0;

            double completionBonus =
                ratingInfo.CompletionBonus ?? 0;

            double difficulty =
                ratingInfo.Difficulty ?? 1.0;

            double platinumBonus =
                ratingInfo.PlatinumBonus ?? 0;

            double platinumDifficulty =
                ratingInfo.PlatinumDifficulty ?? 1.0;

            double ultimateBonus =
                ratingInfo.UltimateBonus ?? 0;

            double ultimateDifficulty =
                ratingInfo.UltimateDifficulty ?? 1.0;

            double standardiser =
                ratingInfo.Standardiser ?? 0;

            double setBaseScore =
                ratingInfo.SetBaseScore ?? 0;

            double multiplier =
                ratingInfo.MultiplierSetBase ?? 1.0;

            double scoreTime;

            if (parTime > 0)
            {
                scoreTime =
                    Math.Min(
                        scoreMs,
                        parTime) / 1000.0;
            }
            else
            {
                scoreTime =
                    scoreMs / 1000.0;
            }

            scoreTime -=
                timeOffset / 1000.0;

            scoreTime += 0.1;

            double bonus = 0;

            if (platinumTime > 0 &&
                scoreMs < platinumTime)
            {
                bonus +=
                    platinumBonus *
                    platinumDifficulty;
            }

            if (ultimateTime > 0 &&
                scoreMs < ultimateTime)
            {
                bonus +=
                    ultimateBonus *
                    ultimateDifficulty;
            }

            completionBonus *= difficulty;

            double rating =
                (
                    completionBonus +
                    bonus +
                    (
                        (Math.Log10(scoreTime) *
                         standardiser) -
                        setBaseScore
                    ) * -1
                ) *
                multiplier;

            if (scoreMs > parTime &&
                parTime > 0)
            {
                double lostPerSecond =
                    (rating - 1) /
                    (
                        MaxTimeSeconds -
                        (parTime / 1000.0)
                    );

                double overPar =
                    Math.Max(
                        scoreMs - parTime,
                        0) / 1000.0;

                rating -=
                    overPar *
                    lostPerSecond;
            }

            return (int)Math.Floor(
                rating < 1
                    ? 1
                    : rating);
        }

        public List<int?> CalculateRatings(
    string level,
    List<int> timesMs)
        {
            List<int?> ratings = new();

            foreach (int timeMs in timesMs)
            {
                // -1 means there is no personal record.
                if (timeMs < 0)
                {
                    ratings.Add(null);
                    continue;
                }

                ratings.Add(
                    CalculateRating(
                        level,
                        timeMs));
            }

            return ratings;
        }
    }
}