using System.Collections.Generic;
using UnityEngine;
using UnitySteer;
using UnitySteer.Helpers;

/// <summary>
/// Steers a vehicle to pursuit another one
/// </summary>
public class SteerForPursuit : Steering
{
	#region Private fields
	[SerializeField]
	Vehicle _quarry;
	
	[SerializeField]
	float _maxPredictionTime = 5;
	#endregion
	
	#region Public properties
	/// <summary>
	/// Maximum time to look ahead for the prediction calculation
	/// </summary>
	public float MaxPredictionTime {
		get {
			return this._maxPredictionTime;
		}
		set {
			_maxPredictionTime = value;
		}
	}
	
	/// <summary>
	/// Target being pursued
	/// </summary>
	public Vehicle Quarry {
		get {
			return this._quarry;
		}
		set {
			_quarry = value;
		}
	}
	#endregion
	
	protected override Vector3 CalculateForce ()
	{
		var force    = Vector3.zero;
		var offset	 = _quarry.transform.position - transform.position;
		var distance = offset.magnitude;
        var radius   = Vehicle.Radius + _quarry.Radius;

		if (distance > radius)
		{
			Vector3 unitOffset = offset / distance;

			// how parallel are the paths of "this" and the quarry
			// (1 means parallel, 0 is pependicular, -1 is anti-parallel)
			float parallelness = Vector3.Dot(transform.forward, _quarry.transform.forward);

			// how "forward" is the direction to the quarry
			// (1 means dead ahead, 0 is directly to the side, -1 is straight back)
			float forwardness = Vector3.Dot(transform.forward, unitOffset);

			float directTravelTime = distance / Vehicle.Speed;
			int f = OpenSteerUtility.intervalComparison (forwardness,  -0.707f, 0.707f);
			int p = OpenSteerUtility.intervalComparison (parallelness, -0.707f, 0.707f);

			float timeFactor = 0;		// to be filled in below

			// Break the pursuit into nine cases, the cross product of the
			// quarry being [ahead, aside, or behind] us and heading
			// [parallel, perpendicular, or anti-parallel] to us.
			switch (f)
			{
				case +1:
					switch (p)
					{
					case +1:		  // ahead, parallel
						timeFactor = 4;
						break;
					case 0:			  // ahead, perpendicular
						timeFactor = 1.8f;
						break;
					case -1:		  // ahead, anti-parallel
						timeFactor = 0.85f;
						break;
					}
					break;
				case 0:
					switch (p)
					{
					case +1:		  // aside, parallel
						timeFactor = 1;
						break;
					case 0:			  // aside, perpendicular
						timeFactor = 0.8f;
						break;
					case -1:		  // aside, anti-parallel
						timeFactor = 4;
						break;
					}
					break;
				case -1:
					switch (p)
					{
					case +1:		  // behind, parallel
						timeFactor = 0.5f;
						break;
					case 0:			  // behind, perpendicular
						timeFactor = 2;
						break;
					case -1:		  // behind, anti-parallel
						timeFactor = 2;
						break;
					}
					break;
			}

			// estimated time until intercept of quarry
			float et = directTravelTime * timeFactor;

			// xxx experiment, if kept, this limit should be an argument
			float etl = (et > _maxPredictionTime) ? _maxPredictionTime : et;

			// estimated position of quarry at intercept
			Vector3 target = _quarry.PredictFuturePosition (etl);

			force = Vehicle.GetSeekVector(target);
		}
		return force;
	}
}