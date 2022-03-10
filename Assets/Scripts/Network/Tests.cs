using SkillsVR.SkillsVR.Network.EC;
using UnityEngine;

public class Tests : APICollection
{
    private void Start()
    {
        Login("kelvin@prideandjoy.org", "Password123!",
            (response) =>
            {
                LoginOrganisation((orgResponse) =>
                {
                    GetConfig(77,
                        (configResponse) => SubmitUserLearningRecord(77, (submitReponse) => Debug.Log(JsonUtility.ToJson(submitReponse)), 
                        (error) => Debug.LogError(error)),
                    (error) => Debug.LogError(error));
                },
                (error) => Debug.LogError(error));
            }, 
            (error) => Debug.LogError(error));
    }
}
