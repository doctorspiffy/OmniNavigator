using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
(C) 2024 Jason Leigh - Laboratory for Advanced Visualization & Applications - University of Hawaii at Manoa

This is a revision of my original OmniNavigator published in the book VR Developer Gems.

This Unity script been updated to be non-specific to an particular VR library.

*/

public class OmniNavigator : MonoBehaviour
{
    public enum rotationLock {None,X,Y,Z};

    [Tooltip("The main VR hub (Camera Rig)")]
	public GameObject mainHub;

	[Tooltip("Your interaction controller")]
    public GameObject wand;

	[Tooltip("The game object representing your head position")]
    public GameObject head;

	[Header("Navigation Settings")]
	[Tooltip("Enable Navigation")]
	public bool enableNavigation = true;

	[Tooltip("Enable collision checking against objects in the scene")]
    public bool enableCollisions = true;
	[Tooltip("Disable Movement in X")]
	public bool disableNavigationX = false;
	[Tooltip("Disable Movement in Y")]
	public bool disableNavigationY = false;
	[Tooltip("Disable Movement in Z")]
	public bool disableNavigationZ = false;


	[Tooltip("Lock rotation about an axis.")]
	public rotationLock lockRotation = rotationLock.None;
	[Tooltip("Movement speed")]
	public float moveSpeed = 5f;
	[Tooltip("Rotation speed")]
	public float rotateSpeed = 1f;
	[Tooltip("Reset navigation speed")]
	public float resetSpeed = 0.5f;
	[Tooltip("Reset navigation position")]
	public Vector3 resetPosition = new Vector3(0,0,0);
	[Tooltip("Reset navigation rotation")]
	public Vector3 resetRotation = new Vector3(0,0,0);
	[Tooltip("Object to show on wand when in navigation mode")]
	public GameObject cursor;
    [Tooltip("Height of character")]
    public float charHeight = 1.78f;
    [Tooltip("Forehead height")]
    public float foreheadHeight = 0.15f;

	bool doNav = false;
    bool doReset = false;

	Vector3 startPosition = new Vector3();
	Quaternion startRotation = Quaternion.identity;
	bool doneNav = false;
	private CharacterController charCont;
	Quaternion resetAngle; 

    // Start is called before the first frame update
    void Start()
    {
        resetAngle = Quaternion.Euler(resetRotation);
    }
void AdjustPlayerCollider() {
		
        if (!enableCollisions) return;
        
		CapsuleCollider playerCollider = mainHub.GetComponent<CapsuleCollider>();
        if (playerCollider == null) return;
		Transform eye = head.transform;
		Vector3 pos = new Vector3 ();

        // Adjust the height of capsule collider to match person height
        float height = eye.localPosition.y +  foreheadHeight;

        pos.x = eye.localPosition.x;
        pos.y = height / 2.0f;
        pos.z = eye.localPosition.z;

        if (height > charHeight) height = charHeight;
        playerCollider.height = height;

		playerCollider.center = pos;
	}

   public  void NavOn(){
        doNav = true;
    }

    public void NavOff(){
        doNav = false;
    }

    public void ResetOn(){
        doReset=true;
    }

   public void ResetOff(){
        doReset = false;
    }


void Navigate() {
		
		if (!enableNavigation)
			return;


        if (doReset){
            mainHub.transform.position = Vector3.Slerp (mainHub.transform.position, resetPosition, resetSpeed * Time.deltaTime);
            mainHub.transform.rotation = Quaternion.Slerp (mainHub.transform.rotation, resetAngle, resetSpeed * Time.deltaTime);
        }
			
		if (doNav == false) {
			doneNav = false;
			if (cursor)
				cursor.SetActive(false);
		}

		if (doNav) {
			// If wand button pressed the first time then record the starting position and orientation of the wand
			if (doneNav == false) {
				startPosition = wand.transform.localPosition;

				doneNav = true;
				startRotation = wand.transform.localRotation;

			} else {

				// Then at each time check the difference between new and old wand position as well as new and old wand orientation.
				// Apply that difference to the character controller to effect navigation.

				Vector3 movement = wand.transform.localPosition - startPosition;

				// If disable navigation in a particular axis is enabled then set movement values to zero.
				if (disableNavigationX) movement.x = 0;
				if (disableNavigationY) movement.y = 0;
				if (disableNavigationZ) movement.z = 0;

				mainHub.transform.Translate(movement * Time.deltaTime * moveSpeed);

				Quaternion newRotation = wand.transform.localRotation;

				// Check if a rotation lock is enabled and handle it
				float axisLockAngle;
				Quaternion rotator = new Quaternion();

				switch(lockRotation){
				case rotationLock.X:
					axisLockAngle = newRotation.eulerAngles.x;
					rotator.eulerAngles = new Vector3(axisLockAngle,0,0);
					startRotation.eulerAngles = new Vector3(startRotation.eulerAngles.x,0,0);
					break;
				case rotationLock.Y:
					axisLockAngle = newRotation.eulerAngles.y;
					rotator.eulerAngles = new Vector3(0,axisLockAngle,0);
					startRotation.eulerAngles = new Vector3(0, startRotation.eulerAngles.y,0);
					break;
				case rotationLock.Z:
					axisLockAngle = newRotation.eulerAngles.z;
					rotator.eulerAngles = new Vector3(0,0,axisLockAngle);
					startRotation.eulerAngles = new Vector3(0,0,startRotation.eulerAngles.z);	
					break;
				default:
					rotator = newRotation;
					break;
				}

				mainHub.transform.localRotation = mainHub.transform.localRotation*
					Quaternion.Slerp(Quaternion.identity,Quaternion.Inverse(startRotation * Quaternion.Inverse(rotator)),Time.deltaTime * rotateSpeed);


				// If there is a cursor object then orient it with the wand position.
				if (cursor) {
					cursor.SetActive(true);
					cursor.transform.position = wand.transform.position;
					cursor.transform.rotation = wand.transform.rotation;

				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		Navigate ();
	    AdjustPlayerCollider ();
	}
}
