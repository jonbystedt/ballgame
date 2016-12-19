using UnityEngine;
using System.Collections;

public class Occluder : MonoBehaviour {

//	public LayerMask layerMsk;
//	public float viewDistance;
//	public float raysFov;
//	public int samples;
//	public bool realtimeShadows;
//	public int hideDelay;
//
//	private Camera cam;
//	private CameraOperator cameraOp;
//	private Camera rayCaster;
//
//	private RaycastHit hit;
//	private Ray r;
//	private Occludee occludee;
//	private int haltonIndex;
//	private float[] hx;
//	private float[] hy;
//	private int pixels;
//
//	void Awake () 
//	{
//		cam = GetComponent<Camera>();
//		cameraOp = GetComponentInParent<CameraOperator>();
//
//		hit = new RaycastHit();
//		if(viewDistance == 0) viewDistance = 100;
//		cam.farClipPlane = viewDistance;
//		haltonIndex = 0;
//
//		if (this.GetComponent<SphereCollider>() == null)
//		{
//			var coll = gameObject.AddComponent<SphereCollider>();
//			coll.radius = 1f;
//			coll.isTrigger = true;
//		}
//	}
//
//	void Start () 
//	{
//		// Set up Halton Sequence
//		pixels = Mathf.FloorToInt(Screen.width * Screen.height / 4f);
//		hx = new float[pixels];
//		hy = new float[pixels];
//
//		for(int i=0; i < pixels; i++)
//		{
//			hx[i] = HaltonSequence(i, 2);
//			hy[i] = HaltonSequence(i, 3);
//		}
//
//		// Set up raycasting camera
//		GameObject goRayCaster = new GameObject("RayCaster");
//		goRayCaster.transform.Translate(transform.position);
//		goRayCaster.transform.rotation = transform.rotation;
//		rayCaster = goRayCaster.AddComponent<Camera>();
//		rayCaster.enabled = false;
//		rayCaster.clearFlags = CameraClearFlags.Nothing;
//		rayCaster.cullingMask = 0;
//		rayCaster.aspect = cam.aspect;
//		rayCaster.nearClipPlane = cam.nearClipPlane;
//		rayCaster.farClipPlane = cam.farClipPlane;
//		rayCaster.fieldOfView = raysFov;
//		goRayCaster.transform.parent = transform;
//	}
//
//	void Update()
//	{
//		for(int k=0; k <= samples; k++)
//		{
//			r = rayCaster.ViewportPointToRay(new Vector3(hx[haltonIndex], hy[haltonIndex], 0f));
//
////			if (cameraOp.FirstPerson)
////			{
////				r.origin = Game.Player.transform.position;
////			}
//
//			r.origin = Game.CameraPosition;
//
//			haltonIndex++;
//			if(haltonIndex >= pixels) haltonIndex = 0;
//
//			if(Physics.Raycast(r, out hit, viewDistance, layerMsk.value))
//			{
//				Unhide(hit.transform, hit);
//			}
//		}
//	}
//
//	private void Unhide(Transform t, RaycastHit hit){
//
//		if(occludee = t.GetComponent<Occludee>()) {
//			occludee.UnHide(hit);
//		}
//		else if(t.parent != null){
//			Unhide(t.parent, hit);
//		}
//	}
//
//	private float HaltonSequence(int index, int b)
//	{
//		float res = 0f;
//		float f = 1f / b;
//		int i = index;
//		while(i > 0)
//		{
//			res = res + f * (i % b);
//			i = Mathf.FloorToInt(i/b);
//			f = f / b;
//		}
//		return res;
//	}
}
