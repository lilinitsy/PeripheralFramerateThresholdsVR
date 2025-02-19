using System.IO;
using TMPro;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class ExperimentManagerRotation : MonoBehaviour
{
	public ExperimentParamsMLE experiment_params; // Reference the ScriptableObject
	public GameObject left_chart;
	public GameObject right_chart;
	public GameObject fixation_textmeshpro;
	public uint attention_level;

	private ConditionConfigMLE condition_config;
	private MLEThresholdEstimator mle_estimator;
	private float trial_start_time = 0.0f;
	private uint frame_count = 0;
	private uint condition_num = 0;
	private bool correct_guess = false;
	private bool this_trial_answered = false;

	// Variables to change attention level colour;
	private TextMeshPro text_component; // Reference to fixation_textmeshpro's textmeshpro's component... very  convoluted...
	private float colour_change_interval = 0.1f;
	private float last_colour_change_time = 0.0f;
	private Color current_colour;
	private char current_letter = ' ';
	private System.Random random;


	void Start()
	{
        //Application.targetFrameRate = (int) Screen.currentResolution.refreshRateRatio.value;
        Application.targetFrameRate = 90;
        random = new System.Random();

		if (experiment_params == null)
		{
			Debug.LogError("ExperimentParams is not assigned");
			return;
		}

		if (!left_chart || !right_chart)
		{
			Debug.LogError("Spheres not assigned");
			return;
		}


		if (!fixation_textmeshpro)
		{
			Debug.LogError("Fixation TextMeshPro gameobject not assigned");
		}

		// Find textmesh pro
		text_component = fixation_textmeshpro.GetComponent<TextMeshPro>();
		if (!text_component)
		{
			Debug.LogError("TextMeshPro component missing from assigned textmeshpro gameobject");
		}

		if (attention_level == 0)
		{
			text_component.text = "+";
		}

		else
		{
			text_component.text = "Attention level: " + attention_level;
		}

		experiment_params.generate_conditions();
		Debug.Log($"Generated {experiment_params.conditions.Count} trials.");

		// Set first condition we'll test
		setup_condition(Random.Range(0, experiment_params.conditions.Count));
	}

	void FixedUpdate()
	{
		// Debug.Log("dt: " + Time.deltaTime.ToString());
		frame_count++;

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			correct_guess = check_guess(KeyCode.LeftArrow);
			save_trial_data(KeyCode.LeftArrow, correct_guess);
			this_trial_answered = true;
			
		}

		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			correct_guess = check_guess(KeyCode.LeftArrow);
			save_trial_data(KeyCode.RightArrow, correct_guess);
			this_trial_answered = true;
		}

		uint left_update_interval = experiment_params.default_framerate / condition_config.trial_config.left_object_frame_rate;
		uint right_update_interval = experiment_params.default_framerate / condition_config.trial_config.right_object_frame_rate;

		if (left_chart != null && right_chart != null)
		{
			// Should they rotate the same direction or opposing??? This does the same way, opposing was weird.
			if (frame_count % left_update_interval == 0)
			{
				left_chart.transform.Rotate(0.0f, 0.0f, condition_config.speed * Time.deltaTime * left_update_interval);
			}

			if (frame_count % right_update_interval == 0)
			{
				right_chart.transform.Rotate(0.0f, 0.0f, condition_config.speed * Time.deltaTime * right_update_interval);
			};
		}


		// Update for attention
		if (attention_level != 0) // why no implicit bool conversions in c#??
		{
			if (Time.time - last_colour_change_time >= colour_change_interval)
			{
				current_colour = current_colour == Color.red ? Color.green : Color.red;

				// TODO: This doesn't handle T only appearing once
				char new_letter;
				do
				{
					new_letter = (char)random.Next('A', 'Z' + 1);
				} while (new_letter == current_letter);

				current_letter = new_letter;

				text_component.color = current_colour;
				text_component.text = current_letter.ToString();

				last_colour_change_time = Time.time;
			}
		}

		if (Time.time - trial_start_time >= 5.0f && this_trial_answered)
		{

			if (condition_config.num_trials >= mle_estimator.max_trials)
			{
				setup_condition(Random.Range(0, experiment_params.conditions.Count));
			}

			else
			{
				setup_trial(correct_guess);
			}
		}
	}



	public void setup_condition(int condition_index)
	{
		if(condition_index < 0 || condition_index >= experiment_params.conditions.Count)
		{
			Debug.LogError("Invalid condition index");
			return;
		}

		// Reset MLE every condition
		mle_estimator = new MLEThresholdEstimator();
		condition_config = experiment_params.conditions[condition_index];
		experiment_params.conditions.RemoveAt(condition_index);


		// Initialize objects with the first conditions
		left_chart.transform.position = condition_config.trial_config.left_object_position;
		right_chart.transform.position = condition_config.trial_config.right_object_position;

		// Reset whether this trial got a response
		correct_guess = false;
		this_trial_answered = false;

		condition_num++;
		trial_start_time = Time.time;
	}


	public void setup_trial(bool previous_correct)
	{
		// Don't update condition_config.num_trials. It'll get updated by set_next_trial()
		// Update MLE
		mle_estimator.record_response(previous_correct);

		float new_eccentricity = mle_estimator.get_current_eccentricity();
		float x_pos = experiment_params.z_position * Mathf.Tan(new_eccentricity * Mathf.Deg2Rad);

		TrialConfigMLE new_trial_config = new TrialConfigMLE
		{
			left_object_position = new Vector3(-x_pos, experiment_params.y_position, experiment_params.z_position),
			right_object_position = new Vector3(x_pos, experiment_params.y_position, experiment_params.z_position),
			eccentricity = new_eccentricity, 
			left_object_frame_rate = condition_config.trial_config.left_object_frame_rate,
			right_object_frame_rate = condition_config.trial_config.right_object_frame_rate
		};

		condition_config.set_next_trial(new_trial_config);

		left_chart.transform.position = condition_config.trial_config.left_object_position;
		right_chart.transform.position = condition_config.trial_config.right_object_position;

		// Reset whether this trial got a response
		correct_guess = false;
		this_trial_answered = false;
		Debug.Log($"Condition {condition_num}, Trial {condition_config.num_trials} | New Eccentricity: {new_eccentricity}°");

		trial_start_time = Time.time;
	}


	public void initialize_fixation_for_attentional_trials()
	{
		current_colour = random.Next(0, 2) == 0 ? Color.red : Color.green;

		do
		{
			current_letter = (char)random.Next('A', 'Z' + 1);
		} while (current_letter != 'T');

		text_component.color = current_colour;
		text_component.text = current_letter.ToString();
	}


	bool check_guess(KeyCode keycode)
	{
		// 
		if(keycode == KeyCode.LeftArrow && condition_config.eye_tested == EyeType.LEFT ||
			keycode == KeyCode.RightArrow && condition_config.eye_tested == EyeType.RIGHT)
		{
			return true;
		}

		return false;
	}



    public void save_trial_data(KeyCode keycode, bool correct)
    {
        string filepath = Path.Combine(Application.dataPath, "experiment-rotation.csv");
        string[] headers = {
			"ConditionNum",
            "TrialNum",
            "Speed",
            "Eccentricity",
            "LeftFrameRate",
            "RightFrameRate",
			"MLE Current Estimate",
			"MLE Step Size",
			"UserInput",
			"Correct", // Whether UserInput matches whichever was the non-target framerate. Will always be false when they're the same.
		};

        string left_right = "";


        if (keycode == KeyCode.LeftArrow)
        {
            left_right = "left";
        }

        else if (keycode == KeyCode.RightArrow)
        {
            left_right = "right";
        }

        string[] trial_data = {
			condition_num.ToString(),
			mle_estimator.trial_count.ToString(),
			condition_config.speed.ToString(),
			condition_config.trial_config.eccentricity.ToString(),
			condition_config.trial_config.left_object_frame_rate.ToString(),
			condition_config.trial_config.right_object_frame_rate.ToString(),
			mle_estimator.current_estimate.ToString(),
			mle_estimator.step_size.ToString(),
			left_right,
			correct.ToString(),
			// something for if their eyes deviated			
        };


        if (!File.Exists(filepath))
        {
            string headerline = string.Join(",", headers) + "\n";
            File.WriteAllText(filepath, headerline);
        }

        // Append trial data
        string trialline = string.Join(",", trial_data) + "\n";
        File.AppendAllText(filepath, trialline);
    }
}
