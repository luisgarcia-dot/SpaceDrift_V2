
using UnityEngine;

public class MovimentNauJugador : MonoBehaviour
{
    // AUDIO
    AudioSource audiosr;
    public AudioClip[] AudioClips;

    // CAÑONES
    public Transform[] canons;

    // MOVIMIENTO
    public float velocitat = 8f;
    public float velocitatRotacio = 180f;

    private Rigidbody rb;
    private float movimentX;
    private float movimentY;

    // DISPARO
    public GameObject projectilPrefab;
    public float velocitatProjectil = 20f;
    public float tempsEntreDispars = 0.25f;

    private float tempsDispar;
   
    public float inclinacioMaxima = 25f;
    public float suavitatRotacio = 5f;

    // START
    void Start()
    {
        
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        audiosr = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        if (audiosr != null)
        {
            audiosr.Play();
        }

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    // UPDATE
    void Update()
    {
        LlegirMoviment();
        ControlarDispar();
    }

    // MOVIMENT
    void LlegirMoviment()
    {
        movimentX = Input.GetAxisRaw("Horizontal");
        movimentY = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        MoureNau();
    }

    void MoureNau()
    {
        Vector3 moviment = new Vector3(movimentX, movimentY, 0f);

        // Evita que la diagonal sigui més ràpida
        moviment = Vector3.ClampMagnitude(moviment, 1f);

        Vector3 novaPosicio = transform.position +
            moviment * velocitat * Time.fixedDeltaTime;

        // Mantenir sempre la mateixa Z
        novaPosicio.z = transform.position.z;

        transform.position = novaPosicio;

        // Girar lleugerament la nau segons el moviment horitzontal
       float rotacioObjectiu = -movimentX * inclinacioMaxima;

        Quaternion rotacioObjectiuQuaternion = Quaternion.Euler(
            0f,
            180f,
            rotacioObjectiu
        );

        transform.rotation = Quaternion.Lerp(
        transform.rotation,
        rotacioObjectiuQuaternion,
        suavitatRotacio * Time.fixedDeltaTime
    );
    }

    // DISPAR
    void ControlarDispar()
    {
        tempsDispar -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) && tempsDispar <= 0f)
        {
            Disparar();
            tempsDispar = tempsEntreDispars;
        }
    }

    void Disparar()
    {
        if (projectilPrefab == null || canons == null)
            return;

        foreach (Transform canon in canons)
        {
            if (canon != null)
            {
                GameObject projectil = Instantiate(
                    projectilPrefab,
                    canon.position,
                    canon.rotation
                );

                Rigidbody rbProjectil = projectil.GetComponent<Rigidbody>();

                if (rbProjectil != null)
                {
                    rbProjectil.linearVelocity =
                        canon.up * velocitatProjectil;
                }
            }
        }
    }

    // COL·LISIONS
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("La nau ha col·lisionat amb un enemic!");

            // Aquí podràs restar vida a la nau
        }

        if (other.CompareTag("EnemyProjectile"))
        {
            Debug.Log("La nau ha rebut un impacte!");

            // Aquí podràs restar vida a la nau
            Destroy(other.gameObject);
        }

        if (other.CompareTag("PowerUp"))
        {
            Debug.Log("Has recollit un PowerUp!");

            Destroy(other.gameObject);
        }
    }
}
