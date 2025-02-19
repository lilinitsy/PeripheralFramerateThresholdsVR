using UnityEngine;
using System;
using System.Collections.Generic;

public class MLEThresholdEstimator
{
	public float current_estimate = 30f; // Start at 30 degrees
	public float sigma = 5.0f; // Slope of the psychometric function
	public float step_size = 10.0f; // How much we move eccentricity per trial
	public int max_trials = 15;
	public int trial_count = 0;

	public List<float> eccentricities_tested = new List<float>();
	public List<int> correct_responses = new List<int>();


	public float get_current_eccentricity()
	{
		return current_estimate;
	}


	public void record_response(bool correct)
	{
		eccentricities_tested.Add(current_estimate);
		correct_responses.Add(correct ? 1 : 0);
		trial_count++;

		if (trial_count >= max_trials)
		{
			Debug.Log($"Final estimated threshold: {compute_threshold()} degrees");
			return;
		}

		update_estimate();
	}

	public void update_estimate()
	{
		float new_estimate = compute_threshold();

		// Reduce step size every update
		step_size = Mathf.Max(0.5f, step_size * 0.8f);

		// Move towards the estimated threshold
		current_estimate += Mathf.Sign(new_estimate - current_estimate) * step_size;
		current_estimate = Mathf.Clamp(current_estimate, 5.0f, 45.0f); // Keep within valid eccentricities
	}

	public float compute_threshold()
	{
		// Uses Maximum Likelihood Estimation to find the eccentricity where 50% detection occurs
		float best_estimate = current_estimate;
		float max_likelihood = float.NegativeInfinity;

		for (float e_t = 15.0f; e_t <= 45.0f; e_t += 0.1f)
		{
			float likelihood = compute_likelihood(e_t);
			if (likelihood > max_likelihood)
			{
				max_likelihood = likelihood;
				best_estimate = e_t;
			}
		}

		return best_estimate;
	}

	public float compute_likelihood(float e_t)
	{
		float likelihood = 0f;
		for (int i = 0; i < eccentricities_tested.Count; i++)
		{
			float e = eccentricities_tested[i];
			int correct = correct_responses[i];

			float prob_correct = 0.5f + 0.5f * Mathf.Exp(-(e - e_t) * (e - e_t) / (2 * sigma * sigma));
			likelihood += correct * Mathf.Log(prob_correct) + (1 - correct) * Mathf.Log(1 - prob_correct);
		}
		return likelihood;
	}
}
