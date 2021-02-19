using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UXF;

public class ResultsController : MonoBehaviour
{
    // Reference to our Session component
    public Session session;

    // An example method to be called when a user gives a response
    public void TrialInfo(string name, int ans)
    {
        // in this example, a user has selected a number, and we want to record it.
        session.CurrentTrial.result["selected_number"] = ans;

        // in our trial_results output, the selected_number column will now be filled in with the selection for each trial.

        // we can assign results either before or after we end the trial.
        session.CurrentTrial.End();
    }

}