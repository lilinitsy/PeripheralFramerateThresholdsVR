using System.IO;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class ExperimentManagerTowards : MonoBehaviour
{
	public ExperimentParams experiment_params; // Reference the ScriptableObject
	public GameObject left_sphere;
	public GameObject right_sphere;

	private TrialConfig trial_config;
	private Vector3 camera_origin;
	private Vector3 left_sphere_target_point;
	private Vector3 right_sphere_target_point;

	private float trial_start_time = 0.0f;
	private uint frame_count = 0;
	private uint current_trial = 0;

	void Start()
	{
		//Application.targetFrameRate = (int) Screen.currentResolution.refreshRateRatio.value;
		Application.targetFrameRate = 120;

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

		experiment_params.generate_trials();
		Debug.Log($"Generated {experiment_params.trials.Count} trials.");
		camera_origin = new Vector3(0.0f, 1.0f, 0.0f);
		left_sphere_target_point = new Vector3(camera_origin.x - 1.0f, camera_origin.y, camera_origin.z);
		right_sphere_target_point = new Vector3(camera_origin.x + 1.0f, camera_origin.y, camera_origin.z);

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

		else if(Input.GetKeyDown(KeyCode.RightArrow))
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


		Debug.Log($"Trial {trial_index}/{experiment_params.trials.Count} - " +
				  $"Speed: {trial_config.speed}, " +
				  $"Left FrameRate: {trial_config.left_object_frame_rate}, " +
				  $"Right FrameRate: {trial_config.right_object_frame_rate}");
	}


	public void save_trial_data(KeyCode keycode)
	{
		string filepath = Path.Combine(Application.dataPath, "experiment-towards.csv");
		string[] headers = {
			"TrialNum",
			"Speed",
			"LeftFrameRate",
			"RightFrameRate",
			"UserInput"
		};

		string left_right = "";
		
		if(keycode == KeyCode.LeftArrow)
		{
			left_right = "left";
		}

		else if(keycode == KeyCode.RightArrow)
		{
			left_right = "right";
		}

		string[] trial_data = {
			current_trial.ToString(),
			trial_config.speed.ToString(),
			trial_config.left_object_frame_rate.ToString(),
			trial_config.right_object_frame_rate.ToString(),
			left_right,
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
