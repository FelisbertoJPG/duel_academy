using UnityEngine;

[RequireComponent(typeof(CharacterController))] // Garante que o GameObject tenha um CharacterController
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")] // Atributo para organizar os campos no Inspector do Unity, criando uma seção chamada "Movimento"
    [SerializeField] private float moveSpeed = 4f; 
    /* Serializar um campo : permite que ele seja editável no Inspector do Unity, mesmo que seja privado. Isso é útil para manter a encapsulação e ainda permitir ajustes fáceis sem expor o campo publicamente. Se é mostrado no Inspector é porque esta sendo serializado, mesmo que seja privado.
    O que um campos Serializadop faz: Permite que a Unity Guarde valores privados entre sessões de edição. memoria Local /Json / PlayerPrefs (algo parecido).
    */
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Gravidade")] //Atributo para organizar os campos no Inspector do Unity, criando uma seção chamada "Gravidade"
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;

    private CharacterController controller; 
    private Vector3 moveDirection = Vector3.zero;
    private float verticalVelocity = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update é chamado uma vez por frame, e é onde a lógica de movimento do jogador é processada.
    // fixedUpdate é chamado em intervalos fixos e é mais adequado para física, mas como estamos usando CharacterController, que não é baseado em física, Update é a escolha correta aqui.
    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        //criado variavel local do tipo vector3 para armazenar a direção de entrada do jogador, usando os eixos horizontal e vertical. O método normalized é usado para garantir que a direção tenha um comprimento de 1, o que é importante para manter a velocidade consistente, independentemente da direção.
        if (inputDirection.sqrMagnitude > 0.001f)
        {
            moveDirection = transform.TransformDirection(inputDirection);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalVelocity = (moveDirection * moveSpeed) + Vector3.up * verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }
}