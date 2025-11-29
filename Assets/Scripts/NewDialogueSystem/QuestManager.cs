using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public int experience;
    public int coins;

    public void AddReward(int exp, int coin)
    {
        experience += exp;
        coins += coin;
        Debug.Log($"Награда: +{exp} XP, +{coin} монет");
    }
}
