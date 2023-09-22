using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

public class HighScore
{
    public int position;
    public string name;
    public double score;
}

public class HighScoreService
{
 
    public async Task<List<HighScore>> FetchHighScores(Secrets secrets)
    {
        var client = new GitHubClient(new ProductHeaderValue("UnityHighScoreApp"));
        client.Credentials = new Credentials(secrets.token);

        var gist = await client.Gist.Get(secrets.gistId);

        var file = gist.Files.Values.FirstOrDefault();
        if (file == null)
        {
            throw new Exception("No file found in the Gist.");
        }

        var json = file.Content;
        var highScores = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HighScore>>(json);

        return highScores?.OrderByDescending(x=> x.score).ToList();
    }

    public async Task UpdateHighScores(List<HighScore> highScores, Secrets secrets)
    {
        var client = new GitHubClient(new ProductHeaderValue("UnityHighScoreApp"));
        client.Credentials = new Credentials(secrets.token);

        // Round the score to two decimal points
        foreach (var score in highScores)
        {
            score.score = Math.Round(score.score, 2);
        }

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(highScores);
        var update = new GistUpdate { Files = { [secrets.gistId] = new GistFileUpdate { Content = json } } };

        await client.Gist.Edit(secrets.gistId, update);
    }

    public List<HighScore> AddHighScore(List<HighScore> highScores, string name, double score)
    {
        var newScore = new HighScore { name = name, score = score };

        // Add the new score to the list
        highScores.Add(newScore);

        // Sort the list in descending order
        highScores = highScores.OrderByDescending(score => score.score).ToList();

        // Keep only the top 10 scores
        highScores = highScores.Take(10).ToList();

        // Assign positions to the scores
        for (int i = 0; i < highScores.Count; i++)
        {
            highScores[i].position = i + 1;
        }

        return highScores;
    }

    public bool IsHighScore(double userScore, List<HighScore> highScores)
    {
        // If there are less than 10 scores, it's a high score
        if (highScores.Count < 10)
        {
            return true;
        }

        // If the user's score is higher than the lowest score, it's a high score
        if (userScore > highScores[highScores.Count - 1].score)
        {
            return true;
        }

        return false;
    }
}
