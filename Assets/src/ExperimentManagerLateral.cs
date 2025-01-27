using UnityEngine;
using TMPro;
using System.IO;
using static System.Runtime.CompilerServices.RuntimeHelpers;


public class ExperimentManagerLateral : MonoBehaviour
{
	public ExperimentParams experiment_params; // Reference the ScriptableObject
	public GameObject left_sphere;
	public GameObject right_sphere;
	public GameObject fixation_textmeshpro;
	public uint attention_level;

	private TrialConfig trial_config;
	private Vector3 camera_origin;
	private Vector3 left_sphere_target_point;
	private Vector3 right_sphere_target_point;

	private float trial_start_time = 0.0f;
	private uint frame_count = 0;
	private uint current_trial = 0;

	// Variables to change attention level colour;
	private TextMeshPro text_component; // Reference to fixation_textmeshpro's textmeshpro's component... very  convoluted...
	private float colour_change_interval = 0.25f;
	private float last_colour_change_time = 0.0f;
	private Color current_colour;
	private char current_letter = ' ';
	private System.Random random;

	void Start()
	{
        //Application.targetFrameRate = (int) Screen.currentResolution.refreshRateRatio.value;
        Application.targetFrameRate = 180;
        random = new System.Random();

		if (experiment_params == null)
		{
			Debug.LogError("ExperimentParams is not assigned");
			return;
		}

		if (!left_sphere || !right_sphere)
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

		experiment_params.generate_trials();
		Debug.Log($"Generated {experiment_params.trials.Count} trials.");
		camera_origin = new Vector3(0.0f, 1.0f, 0.0f);

		left_sphere_target_point = new Vector3(camera_origin.x - 2.0f, camera_origin.y, experiment_params.z_position);
		right_sphere_target_point = new Vector3(camera_origin.x + 2.0f, camera_origin.y, experiment_params.z_position);

		SetupTrial(Random.Range(0, experiment_params.trials.Count));
	}

	void FixedUpdate()
	{
		// Debug.Log("dt: " + Time.deltaTime.ToString());
		frame_count++;

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			save_trial_data(KeyCode.LeftArrow);
		}

		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			save_trial_data(KeyCode.RightArrow);
		}

		int left_update_interval = experiment_params.default_framerate / trial_config.left_object_frame_rate;
		int right_update_interval = experiment_params.default_framerate / trial_config.right_object_frame_rate;

		if (left_sphere != null && right_sphere != null)
		{

			if (frame_count % left_update_interval == 0)
			{
				left_sphere.transform.position = Vector3.MoveTowards(
					left_sphere.transform.position,
					left_sphere_target_point,
					trial_config.speed * Time.deltaTime * left_update_interval
				);
			}

			if (frame_count % right_update_interval == 0)
			{
				right_sphere.transform.position = Vector3.MoveTowards(
					right_sphere.transform.position,
					right_sphere_target_point,
					trial_config.speed * Time.deltaTime * right_update_interval
				);
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

		if (Time.time - trial_start_time >= 5.0f)
		{
			SetupTrial(Random.Range(0, experiment_params.trials.Count));
		}
	}

	public void SetupTrial(int trial_index)
	{
		current_trial++;
		if (trial_index < 0 || trial_index >= experiment_params.trials.Count)
		{
			Debug.LogError("Invalid trial index.");
			return;
		}

		trial_config = experiment_params.trials[trial_index];
		experiment_params.trials.RemoveAt(trial_index);

		left_sphere.transform.position = trial_config.left_object_position;
		left_sphere.name = "Left Sphere";

		right_sphere.transform.position = trial_config.right_object_position;
		right_sphere.name = "Right Sphere";

		trial_start_time = Time.time;

		initialize_fixation_for_attentional_trials();


		Debug.Log($"Trial {trial_index}/{experiment_params.trials.Count} - " +
				  $"Speed: {trial_config.speed}, " +
				  $"Left FrameRate: {trial_config.left_object_frame_rate}, " +
				  $"Right FrameRate: {trial_config.right_object_frame_rate}");
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

    public void save_trial_data(KeyCode keycode)
    {
        string filepath = Path.Combine(Application.dataPath, "experiment-lateral.csv");
        string[] headers = {
            "TrialNum",
            "Speed",
            "Eccentricity",
            "LeftFrameRate",
            "RightFrameRate",
            "UserInput",
            "Correct" // Whether UserInput matches whichever was the non-target framerate. Will always be false when they're the same.
		};

        string left_right = "";
        bool b_correct = false;


        if (keycode == KeyCode.LeftArrow)
        {
            left_right = "left";
        }

        else if (keycode == KeyCode.RightArrow)
        {
            left_right = "right";
        }

        // Left is slower, they selected left
        if (trial_config.left_object_frame_rate != experiment_params.default_framerate && left_right == "left")
        {
            b_correct = true;
        }

        else if (trial_config.right_object_frame_rate != experiment_params.default_framerate && left_right == "right")
        {
            b_correct = true;
        }



        string[] trial_data = {
            current_trial.ToString(),
            trial_config.speed.ToString(),
            trial_config.eccentricity.ToString(),
            trial_config.left_object_frame_rate.ToString(),
            trial_config.right_object_frame_rate.ToString(),
            left_right,
            b_correct.ToString(),
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
