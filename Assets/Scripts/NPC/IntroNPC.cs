using UnityEngine;

[RequireComponent(typeof(NPCTeleportInteraction))]
public class IntroNPC : MonoBehaviour
{
    private NPCTeleportInteraction npcInteraction;

    void Start()
    {
        npcInteraction = GetComponent<NPCTeleportInteraction>();
        AddIntroScenario();
    }

    private void AddIntroScenario()
    {
        DialogScenario scenario = new DialogScenario
        {
            type = DialogType.Monologue,
            speakerName = "Наставник",
            monologueLines = new string[]
            {
                "Привет! Здорово, что ты пришел!",
                "У нас 20 минут, чтобы покушать с нами, набраться сил и получить первые очки опыта.",
                "В будущем когда ты поднимешь свой уровень, нас с тобой ждут подземелья и леса, полные приключений и даже опасностей. С которыми, ты возможно справишься!",
                "",
                "Прежде чем ты сможешь сделать заказ на английском, тебе нужно пройти пару заданий, набраться опыта и повысить свой уровень.",
                "",
                "Выучи 2 слова-действия (глагола), за это ты получишь первые монетки и опыт, чтобы повысить свой уровень!",
                "<b>want</b> - хотеть",
                "<b>learn</b> - учить"
            }
        };

        npcInteraction.AddDialogScenario(scenario);
    }
}
