using UnityEngine;

namespace AC.LSky
{

	[AddComponentMenu ("AC/LSky/Time Of Day")]
	[ExecuteInEditMode]	public class LSkyTOD : MonoBehaviour
	{
		public bool playTime = true; // Progress time.
		public float dayInSeconds = 900;  // 60*15 = 900 (15 minutes).
		[Range(0,1)] public float TOD = .5f;

		const int k_HoursPerDay = 24;   
		
		void OnDaySwitch () {
			TOD = 0;	
		}
		
		void ProgressTime()
		{
			// timeline = Mathf.Repeat (timeline, k_HoursPerDay);
			// Add time.
			if (Application.isPlaying && playTime && dayInSeconds != 0)
			{
				TOD += (Time.deltaTime / dayInSeconds);
				if (TOD >= 1.0f) {
					OnDaySwitch();
				}
			}
		}

		private LSky m_SkyManager = null;
		
		[Range(-180f, 180f)] public float orientation = 0.0f;

		float XRot{ get{ return TOD * 360f; } }
		Quaternion SunRotation { get { return Quaternion.Euler(XRot - 90f, 0, 0); } }
		Quaternion MoonRotationOpposiveSun { get { return Quaternion.Euler (XRot - 270f, 0, 0); } }

		public int CurrentHour{ get{ return (int)Mathf.Floor(TOD * k_HoursPerDay); } }		
		public int CurrentMinute{ get{ return (int)Mathf.Floor( (TOD * k_HoursPerDay - CurrentHour) * 60); } }


		void Update()
		{
			if (m_SkyManager == null) m_SkyManager = GetComponent<LSky>();
		
			ProgressTime();
			m_SkyManager.SetSunLightLocalRotation(SunRotation);
			m_SkyManager.SetMoonLightLocalRotation( MoonRotationOpposiveSun );

			transform.localEulerAngles = new Vector3(0.0f, orientation, 0.0f);
		}


		public string GetTimeString
		{
			get
			{
				string h = CurrentHour < 10 ? "0" + CurrentHour.ToString () : CurrentHour.ToString ();
				string m = CurrentMinute < 10 ? "0" + CurrentMinute.ToString () : CurrentMinute.ToString ();
				
				return h + ":" + m;
			}
		}
	}
}