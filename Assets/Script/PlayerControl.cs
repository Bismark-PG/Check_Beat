using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    void Update()
    {
        int x = 0;
        int z = 0;

        if (Input.GetKeyDown(KeyCode.W))
        {
            z = 1;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        { 
            z = -1; 
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            x = -1;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        { 
            x = 1;
        }

        if (x != 0 || z != 0)
        {
            GameManager.HitResult hitResult = GameManager.Instance.TryAction();

            if (hitResult != GameManager.HitResult.Miss)
            {
                TryMove(x, z);
            }

            if (RhythmUIManager.Instance != null)
            {
                RhythmUIManager.Instance.ShowHitPopup(hitResult);
            }
        }
    }

    void TryMove(int x, int z)
    {
        Vector3 targetPos = transform.position + new Vector3(x, 0, z);

        // Check Map Boundary
        if (targetPos.x < 0f || targetPos.x > MapManager.Instance.currentWidth ||
            targetPos.z < 0f || targetPos.z > MapManager.Instance.currentHeight)
        {
            Debug.Log("Move Failed : Out Of Map Range!");
            return;
        }

        // Make Sphere Check for Collision at Target Position
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, 0.3f);
        bool isBlocked = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Wall_B") || hitCollider.CompareTag("Wall_W"))
            {
                isBlocked = true;
                break;
            }
        }

        if (isBlocked)
        {
            Debug.Log("\"Move Failed : Blocked by Wall!");
        }
        else
        {
            transform.position = targetPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trigger For Enemy
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Game Over!");
            UIManager.Instance.ShowGameOver(StageManager.Instance.currentStage);
            this.enabled = false;
        }
        // Trigger For Exit
        else if (other.CompareTag("Exit"))
        {
            StageManager.Instance.StageClear();
        }
    }
}
