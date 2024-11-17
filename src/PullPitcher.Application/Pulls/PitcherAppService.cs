using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace PullPitcher.Pulls
{
    [RemoteService(false)]
    public class PitcherAppService : PullPitcherAppService, IPitcherAppService
    {
        private Pitcher _pitcher;
        // TODO: Move to settings
        private Regex _pullRequestRegex = new Regex(@"https:\/\/dev\.azure\.com\/([^\/]+)\/([^\/]+)\/_git\/([^\/]+)\/pullrequest\/(\d+)");
        public PitcherAppService(Pitcher pitcher)
        {
            _pitcher = pitcher;
        }
        public async Task<List<PullReviewerDto>> Pitch(string pullRequestLink, string ownerId)
        {
            // TODO Create Parser
            Logger.LogInformation("Pitch Request: {pullRequestLink} {ownerId}", pullRequestLink, ownerId);
            Match match = _pullRequestRegex.Match(pullRequestLink);
            if (match.Success)
            {
                Logger.LogDebug("Request Progress: Link Matched");
                var Organization = match.Groups[1].Value;
                Logger.LogDebug("Request Progress: Organization:{Organization}", Organization);
                var Project = match.Groups[2].Value;
                Logger.LogDebug("Request Progress: Project:{Project}", Project);
                var Repo = match.Groups[3].Value;
                Logger.LogDebug("Request Progress: Repo:{Repo}", Repo);
                var PullRequestNumber = int.Parse(match.Groups[4].Value);
                Logger.LogDebug("Request Progress: PullRequestNumber:{PullRequestNumber}", PullRequestNumber);
                string key = $"{Organization}*{Project}*{Repo}";
                Logger.LogDebug("Request Progress: key:{key}", key);


                var reviewers = await _pitcher.Pitch(pullRequestLink, key, ownerId, PullRequestNumber.ToString());

                Logger.LogInformation($"Pitch Request: Completed Reviewers list {string.Join(", ", reviewers.Select(r => r.CatcherId))}");
                return ObjectMapper.Map<List<PullReviewer>, List<PullReviewerDto>>(reviewers);
            }
            else
            {
                throw new BusinessException(message: "Invalid Pull Request Link");
            }
        }
    }
}
