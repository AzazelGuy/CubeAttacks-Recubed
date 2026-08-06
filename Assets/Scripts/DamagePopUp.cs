using UnityEngine;
using TMPro;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro textMesh;
    private static readonly string[] messages =
        {

        "Nice Try.",
        "Nope.",
        "Not Even Close.",
        "Stayin Alive, Stayin Alive.",
        "Dojyan~~~~n.",
        "I've heard you're pretty strong...",
        "Your power level is no match for mine.",
        "Call me MJ 'cause I'm still alive. (Supposedly.)",
        "Wanna see me do it again?",
        "Didn't Feel That.",
        "Tickles.",
        "Cute.",
        "Bruh.",
        "Really?",
        "Is That All?",
        "Oh great havens!",
        "Hey Peetah, remember the time I was immortal?",
        "YO MAMA SO.. FAT! When she got shot, the bullets BOUNCED off her.",
        "The bullet was taken to the SUPER BOWL, YEAAHHHH!",
        "Reminds me of- XBOX LIVE.",
        "You hit my fat ass >:(",
        "Yummy!",
        "IMMUNE",
        "0 Damage.",
        "Skill Issue.",
        "Pathetic.",
        "Too Weak.",
        "You Call That Damage?",
        "Try Harder.",
        "Missed My Soul.",
        "Death Not Found.",
        "Error 404: HP Lost.",
        "Fuck off",
        "Shit, you crazy?",
        "Plot Armor.",
        "Developer's Favorite.",
        "Batman would be proud.",
        "It's not very effective!",
        "Invincibility.exe Running.",
        "Working as Intended.",
        "Can't Touch This.",
        "Lore Accurate.",
        "Balance? Never Heard of It.",
        "Ow... Anyway.",
        "That Was Adorable.",
        "Keep Going, I'm Watching.",
        "You Done?",
        "I'm Literally Immortal.",
        "This Is Getting Embarrassing.",
        "I'm written by CwC",
        "Now this looks like a job for me.",
        "FUCK YOU PLAYER!",
        "1850 damage- jk.",
        "Needs more Viagra.",
        "5 Bullets in my ass",
        "Guess who's back, back again.",
        "Oh? You're approaching me?",
        "Nha",
        "NAN",
        "It'd be so empty without me.",
        "Groovy...",
        "Dammmmm, I look good...",
        "I Forgot to Take Damage.",
        "Bulletproof",
        "50 Cent would be proud.",
        "Not Sugarcoating It.",
        "+ Parry",
        "Nuh-uh.",
        "/gamemode creative",
        "'- This is bullshit!' I hear you say.",
        "I kinda travelled back in time and dodged it.",
        "Too sexy to die~.",
        "'Ouch!' Cried Bullet. And died."
    };

    private static readonly string[] rareMessages =
        {
        "The Dev Forgot to Add My HP.",
        "Stop Farming Me.",
        "Achievement Unlocked: Wasted Effort.",
        "Your DPS Is Emotional.",
        "Congratulations! Nothing Happened.",
        "*yawn*",
        "Please Continue.",
        "I've Seen Mosquitoes Hit Harder.",
        "Damage.exe Has Stopped Working.",
        "Git Gud.",
        "I changed the damage amount to 0",
        "Ermmm Achssstuallllyyyy I can't dieeeee.",
        "Come to mommy~",
        "Congratulations. You found the rarest popup.",
        "...Okay, THAT was lucky.",
        "Developer Note: Go touch grass.",
        "This message is rarer than my death.",
        "Why are you still hitting me?",
        "There's literally a 0.01% chance of seeing this.",
        "You won the RNG lottery.",
        "My name is Yoshikage Kira. I'm 33 years old. My house is in the northeast section of Morioh, where all the villas are, and I am not married. I work as an employee for the Kame Yu department stores, and I get home every day by 8 PM at the latest. I don't smoke, but I occasionally drink. I'm in bed by 11 PM, and make sure I get eight hours of sleep, no matter what. After having a glass of warm milk and doing about twenty minutes of stretches before going to bed, I usually have no problems sleeping until morning. Just like a baby, I wake up without any fatigue or stress in the morning. I was told there were no issues at my last check-up. I'm trying to explain that I'm a person who wishes to live a very quiet life. I take care not to trouble myself with any enemies, like winning and losing, that would cause me to lose sleep at night. That is how I deal with society, and I know that is what brings me happiness. Although, if I were to fight I wouldn't lose to anyone.",
        "Hope you didn't get the long one",
        "Hit me baby one more time ;3",
        "'The Developer got too tired of making these and will return soon enough.'"
    };

    private float disappearTimer = 1f;
    [SerializeField] private Color textColor;
    [SerializeField] private Gradient CRITICALColor;

    [SerializeField] private float moveYSpeed = 2f;
    [SerializeField] private float disappearSpeed = 3f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // Fun��o para configurar o valor do dano
    public void Setup(float damageAmount)
    {
        textMesh.SetText($"{damageAmount:.00}");
        textColor = textMesh.color;
    }
    public void SetupCrit()
    {
        textMesh.SetText($"Critical!");
        textColor = textMesh.color;
    }
    public void ONCRIT()
    {
        Color top = CRITICALColor.Evaluate(0f);
        Color bottom = CRITICALColor.Evaluate(0f);

        textMesh.rectTransform.sizeDelta = new Vector2(textMesh.rectTransform.sizeDelta.x * 2f, textMesh.rectTransform.sizeDelta.y * 2f);
        textMesh.colorGradient = new VertexGradient(top, top, bottom, bottom);
        textColor = textMesh.color;
    }

    public void Imortal()
    {
        disappearTimer *= 3f;

        // 1% de chance de aparecer uma mensagem rara
        string message = Random.Range(0, 100) == 0
            ? rareMessages[Random.Range(0, rareMessages.Length)]
            : messages[Random.Range(0, messages.Length)];

        textMesh.SetText($"{message}");
        Color top = Color.white;
        Color bottom = Color.gray;
        textMesh.colorGradient = new VertexGradient(top, top, bottom, bottom);
        textColor = textMesh.color;
    }

    private void Update()
    {
        // Faz o texto subir
        transform.position += new Vector3(0, moveYSpeed, 0) * Time.deltaTime;

        // Controla o tempo para começar a sumir
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Efeito de Fade Out (diminuir o Alpha do texto)
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            // Destr�i o objeto quando estiver totalmente invis�vel
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}