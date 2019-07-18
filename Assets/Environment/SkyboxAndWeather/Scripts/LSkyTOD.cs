using UnityEngine;

namespace AC.LSky
{

	[AddComponentMenu ("AC/LSky/Time Of Day")]
	[ExecuteInEditMode]	public class LSkyTOD : MonoBehaviour
	{
		public    bool  playTime      = true; // Progress time.
		public    float dayInSeconds  = 900;  // 60*15 = 900 (15 minutes).
		protected const int k_HoursPerDay = 24;   





		[Range(0,1)] public float TOD = .5f;
		
		// [Range(0.0f, 24f)] public float timeline = 7.0f;



		void OnDaySwitch () {
			TOD = 0;
			
		}
		

		void ProgressTime()
		{
			// timeline = Mathf.Repeat (timeline, k_HoursPerDay);

			// Add time.
			if (playTime && Application.isPlaying && dayInSeconds != 0)
			{
				// timeline += (Time.deltaTime / dayInSeconds) * k_HoursPerDay; 
				TOD += (Time.deltaTime / dayInSeconds);

				if (TOD >= 1.0f) {
					OnDaySwitch();
				}
			}
		}

		private LSky m_SkyManager = null;
		private Transform m_Transform = null;

		[Range(-180f, 180f)] public float orientation = 0.0f;

		// public float XRot{ get{ return timeline * (360f / k_HoursPerDay); } }

		float XRot{ get{ return TOD * 360f; } }
		Quaternion SunRotation { get { return Quaternion.Euler(XRot - 90f, 0, 0); } }
		Quaternion MoonRotationOpposiveSun { get { return Quaternion.Euler (XRot - 270f, 0, 0); } }

		// public int CurrentHour{ get{ return (int)Mathf.Floor(timeline); } }
		public int CurrentHour{ get{ return (int)Mathf.Floor(TOD * k_HoursPerDay); } }
		
		// public int CurrentMinute{ get{ return (int)Mathf.Floor( (timeline - CurrentHour) * 60); } }
		public int CurrentMinute{ get{ return (int)Mathf.Floor( (TOD * k_HoursPerDay - CurrentHour) * 60); } }


		void Update()
		{

			if(!CheckComponents())
			{
				m_Transform  = this.transform;
				m_SkyManager = GetComponent<LSky>();
				return;
			}
				
			ProgressTime();
			m_SkyManager.SetSunLightLocalRotation(SunRotation);
			m_SkyManager.SetMoonLightLocalRotation( MoonRotationOpposiveSun );

			m_Transform.localEulerAngles = new Vector3(0.0f, orientation, 0.0f);
		}

		bool CheckComponents()
		{
			if(m_Transform == null)
				return false;
			if(m_SkyManager == null)
				return false;
			// if(!m_SkyManager.IsReady)
			// 	return false;
			return true;
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