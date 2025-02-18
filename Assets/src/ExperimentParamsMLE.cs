using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

[CreateAssetMenu(fileName = "ExperimentParamsMLE", menuName = "Scriptable Objects/ExperimentParamsMLE")]
public class ExperimentParamsMLE : ScriptableObject
{
	// Parameters should be set in the editor if they need to vary from default
	// Default is set up to work with ExperimentManagerTowards
	public uint default_framerate = 90;
	public float default_start_eccentricity = 30.0f;
	public Vector3 camera_position = new Vector3(0.0f, 1.0f, 0.0f);

	public float y_position = 1.0f;
	public float z_position = 25.0f;

	// Input as degrees for the rotation parameters
	public float[] speeds = { 10.0f };

	public uint[] alternate_framerates = { 90, 45, 30, 15 };

	// operate on these conditions
	public List<ConditionConfigMLE> conditions = new List<ConditionConfigMLE>();


	private float compute_x_position(float eccentricity)
	{
		return z_position * Mathf.Tan(eccentricity * Mathf.Deg2Rad);
	}


	// Deprecated
	public void generate_conditions()
	{
		conditions.Clear();

		float start_x_position = compute_x_position(30.0f);
		foreach (float s in speeds)
		{
			foreach (uint alt_framerate in alternate_framerates)
			{
				TrialConfigMLE right_trial = new TrialConfigMLE
				{
					left_object_position = new Vector3(-start_x_position, y_position, z_position),
					right_object_position = new Vector3(start_x_position, y_position, z_position),
					eccentricity = default_start_eccentricity,
					speed = s,
					left_object_frame_rate = default_framerate,
					right_object_frame_rate = alt_framerate
				};

				TrialConfigMLE left_trial = new TrialConfigMLE
				{
					left_object_position = new Vector3(-start_x_position, y_position, z_position),
					right_object_position = new Vector3(start_x_position, y_position, z_position),
					eccentricity = default_start_eccentricity,
					speed = s,
					left_object_frame_rate = alt_framerate,
					right_object_frame_rate = default_framerate
				};

				conditions.Add(new ConditionConfigMLE
				{
					eye_tested = EyeType.RIGHT,
					trial_config = right_trial,
					framerate = alt_framerate,
					speed = s,
					condition_over = false
				});

				conditions.Add(new ConditionConfigMLE
				{
					eye_tested = EyeType.LEFT,
					trial_config = left_trial,
					framerate = alt_framerate,
					speed = s,
					condition_over = false
				});
			}
		}
	}
}


public enum EyeType
{
	LEFT,
	RIGHT,
	EYETESTED_COUNT,
};

public class ConditionConfigMLE
{
	public EyeType eye_tested;
	public TrialConfigMLE trial_config;
	public uint framerate; // which framerate is this condition testing?
	public float speed;
	public bool condition_over = false;
}

public class TrialConfigMLE
{
	public Vector3 left_object_position; 
	public Vector3 right_object_position; 
	public float eccentricity; // absolute, doesn't indicate left/right
	public float speed;
	public uint left_object_frame_rate;
	public uint right_object_frame_rate;
}
