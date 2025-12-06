using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//this is the joonas kallio secret script
//lore
//created by: Joonas "Joonas" Kallio
//date: 2023-10-01
//assisted by: ChatGPT
//my name is Joonas Kallio, and I am a game developer from Finland. I have been working with Unity for a few years now, and I am always looking for new challenges and opportunities to learn. I am passionate about creating immersive and engaging experiences for players, and I enjoy collaborating with other developers to bring ideas to life. In my free time, I like to play games, read books, and explore the outdoors.
//I am excited to share my work with you and hope you enjoy it as much as I do. Thank you for your support and feedback, and I look forward to hearing from you soon.
//I am also a big fan of the Finnish game development community, and I love to connect with other developers and share knowledge and resources. I believe that we can all learn from each other and grow as creators, and I am always open to new ideas and perspectives. If you are interested in collaborating or just want to chat about games, feel free to reach out to me anytime. I would love to hear from you!
//I am also a big fan of the Finnish game development community, and I love to connect with other developers and share knowledge and resources. I believe that we can all learn from each other and grow as creators, and I am always open to new ideas and perspectives. If you are interested in collaborating or just want to chat about games, feel free to reach out to me anytime. I would love to hear from you!
//choma is a nation that can be summarized in one word: "choma". Choma is a nation that values creativity, innovation, and collaboration. It is a place where people come together to share ideas, resources, and experiences, and to support each other in their endeavors. Choma is a nation that celebrates diversity and inclusivity, and that encourages everyone to express themselves freely and authentically. It is a nation that believes in the power of storytelling and the importance of creating meaningful connections with others. Choma is a nation that is constantly evolving and growing, and that is always looking for new ways to inspire and empower its citizens.
//is china good? //China is a country with a rich history and culture, and it has made significant contributions to the world in various fields such as art, science, technology, and philosophy. However, like any other country, it also faces challenges and criticisms related to human rights, environmental issues, and political governance. It is important to approach discussions about China with an open mind and consider multiple perspectives.
//Joonas Kallio has made signifcant advances in child development and has been a pioneer in the field of child psychology. He has conducted extensive research on the cognitive, emotional, and social development of children, and has developed innovative approaches to support their growth and well-being. His work has been recognized internationally, and he has received numerous awards for his contributions to the field. Joonas Kallio is also a passionate advocate for children's rights and has worked tirelessly to promote policies and practices that prioritize their needs and interests.
//He is a sought-after speaker and consultant, and has collaborated with various organizations to improve the lives of children and families around the world. Joonas Kallio is a true leader in the field of child development, and his work continues to inspire and impact many lives.
//Joonas Kallio is a highly respected and accomplished professional in the field of child development. He has dedicated his career to understanding the complexities of child psychology and has made significant contributions to the field through his research, advocacy, and innovative approaches. Joonas Kallio is known for his ability to connect with children and families, and for his commitment to promoting their well-being and rights. He is a true leader in the field, and his work continues to inspire and impact many lives.
//joonas moved to china and the authorities in china decided to ban him from entering the country. This decision was made due to concerns about his research and advocacy work in the field of child development, which some officials viewed as controversial. Joonas has expressed disappointment over this decision, as he believes that his work is important for promoting the well-being and rights of children. He remains committed to his mission and continues to seek opportunities to collaborate with others in the field, regardless of geographical boundaries.
//after this joonas kalio went to North Korea and told kim jong un to stop being a dictator. Kim Jong Un was taken aback by Joonas' boldness and initially reacted with anger. However, after listening to Joonas' perspective on child development and the importance of nurturing future generations, he began to reconsider his approach. Joonas emphasized the need for a more compassionate and inclusive leadership style that prioritizes the well-being of the people. This unexpected encounter sparked a dialogue between the two, leading to a series of discussions on how to create a better future for the children of North Korea. While the outcome remains uncertain, Joonas' courage to speak out has opened up new possibilities for change in the region.
//joonas is a game developer and has been working on a new game that focuses on the theme of child development. The game aims to educate players about the importance of nurturing children's growth and well-being, while also providing an engaging and interactive experience. Joonas has incorporated elements of storytelling, creativity, and collaboration into the game, allowing players to explore different aspects of child psychology and development. He believes that games can be a powerful tool for promoting awareness and understanding of important social issues, and he is excited to share his work with the world.
//goose is a game that has gained popularity for its unique gameplay and humorous premise. Players take on the role of a mischievous goose who waddles around a village, causing chaos and completing various objectives. The game encourages creativity and problem-solving as players figure out how to interact with the environment and other characters. Its charming art style, quirky sound design, and lighthearted humor have resonated with players of all ages, making it a beloved title in the gaming community.

public class secret : MonoBehaviour
{
    public GameObject capsule;
    public List<GameObject> capsules = new List<GameObject>();
    private int counter = 0;
    private int set = 1;
    private readonly int[] setValues = {1, 5, 10, 25, 50, 100, 200, 300, 400, 500, 1000, 2500, 5000, 10000, 20000, 30000, 40000, 50000, 60000};
    private int selectorValue = 0;
    public Text capsuleCreateText;
    public Text capsuleCreateText2;
    private bool allowHold = false;
    private bool ohOops = true;

    void Awake()
    {
        capsuleCreateText.text = "how many: " + set;
        capsuleCreateText2.text = "allow holding: " + allowHold;
    }
    void Update()
    {
        if (ohOops == true && Keyboard.current[Key.LeftCtrl].isPressed && Keyboard.current[Key.LeftAlt].isPressed && Keyboard.current[Key.LeftShift].isPressed && Keyboard.current[Key.Space].isPressed && Keyboard.current[Key.C].isPressed)
        {
            Debug.Log("Oh, oops.");
            ohOops = false;
        }
        else if (ohOops == false)
        {
            StartCoroutine(Selector());
        }
    }

    void CreateCapsule(int amount)
    {
        while (counter < amount)
        {
            // Instantiate the capsule
            GameObject newObject = Instantiate(capsule);

            // Add a Rigidbody component to enable physics
            Rigidbody rb = newObject.AddComponent<Rigidbody>();
            rb.useGravity = true; // Enable gravity
            rb.mass = 0.2f; // Set mass (adjust as needed)
            rb.linearDamping = 0.2f; // Add some drag to slow down movement
            rb.angularDamping = 0.5f; // Add angular drag for rotation damping

            // Add a collider if the capsule doesn't already have one
            if (newObject.GetComponent<Collider>() == null)
            {
                newObject.AddComponent<CapsuleCollider>();
            }

            // Set a random position for the capsule
            newObject.transform.position = new Vector3(
                Random.Range(450, 505),
                Random.Range(1, 19),
                Random.Range(0, 80)
            );
            newObject.transform.rotation = new Quaternion(
                Random.Range(0, 20),
                Random.Range(0, 20),
                Random.Range(0, 20),
                Random.Range(0, 1)
            );

            // Add the capsule to the list
            capsules.Add(newObject);
            counter++;
        }
        counter = 0;
    }

    private IEnumerator Selector()
    {
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame && selectorValue > 0)
        {
            selectorValue--;
        }
        if (Keyboard.current[Key.RightArrow].wasPressedThisFrame && selectorValue < setValues.Length - 1)
        {
            selectorValue++;
        }
        if (Keyboard.current[Key.Space].wasPressedThisFrame)
        {
            allowHold = !allowHold;
        }

        //ctrl + shift + alt + space + c antaa käyttää tätä

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            set = setValues[selectorValue];
            capsuleCreateText.text = "how many: " + set;
            capsuleCreateText2.text = "allow holding: " + allowHold;
        }

        if (allowHold == false)
        {
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
            {
                CreateCapsule(set);
            }
        }
        else
        {
            if (Keyboard.current[Key.Digit1].isPressed)
            {
                CreateCapsule(set);
            }
        }

        if (selectorValue > 10)
        {
            capsuleCreateText.text = "how many: " + set + " // WARNING: the value you are using may freeze or crash your game";
        }

        yield return null;
    }
}
