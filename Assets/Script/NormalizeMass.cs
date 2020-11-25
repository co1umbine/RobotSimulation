using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalizeMass : MonoBehaviour
{
    private void Apply(Transform root)
    {
        var j = root.GetComponent<HingeJoint>();

        // Apply the inertia scaling if possible
        if (j && j.connectedBody)
        {
            // Make sure that both of the connected bodies will be moved by the solver with equal speed
            j.massScale = j.connectedBody.mass / root.GetComponent<Rigidbody>().mass;
            j.connectedMassScale = j.connectedBody.mass / root.GetComponent<Rigidbody>().mass;
        }

        // Continue for all children...
        for (int childId = 0; childId < root.childCount; ++childId)
        {
            Apply(root.GetChild(childId));
        }
    }

    public void Start()
    {
        Apply(this.transform);
    }
}