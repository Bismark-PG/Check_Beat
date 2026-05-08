using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public enum EnemyType { Normal, Patrol }

    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Normal;
    public float detectRange = 3f;

    [Header("Action Intervals")]
    public int defaultInterval = 2;
    public int chaseInterval = 1;
    private int currentBeatWait = 0;

    [Header("Patrol Settings")]
    public int patrolSteps = 1;  // How Many Steps Before Turning
    private int turnCount = 0;   
    private int currentPatrolStep = 0; 
    private int dirIndex = 0;
    private bool isAlerted = false;

    private Vector3[] ccwDirections = new Vector3[]
    {
        new Vector3(0, 0, 1),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 0)
    };

    void Start()
    {
        GameManager.Instance.OnTurnPassed += OnTurnAction;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTurnPassed -= OnTurnAction;
        }
    }

    void OnTurnAction()
    {
        currentBeatWait++;

        Transform player = GameManager.Instance.playerTransform;
        float dist = Vector3.Distance(transform.position, player.position);

        bool isChasing = dist <= detectRange; // Range Check

        int currentRequiredInterval = isChasing ? chaseInterval : defaultInterval; // Interval Check

        if (currentBeatWait < currentRequiredInterval)
        {
            return;
        }

        currentBeatWait = 0; // Reset Beat Wait After Action

        if (isChasing) // Chaase Mode
        {
            if (!isAlerted)
            {
                isAlerted = true;
                Debug.Log($"{gameObject.name} : Player Found! Wait 1 Action!");
                return;
            }

            ChasePlayer(player.position);
        }
        else // Default Mode
        {
            isAlerted = false; // Chase Mode Reset

            if (enemyType == EnemyType.Patrol)
            {
                PatrolMove();
            }
        }
    }

    void ChasePlayer(Vector3 targetPosition)
    {
            // Chexk Distance To Player
            Vector3 diff = targetPosition - transform.position;

            int dirX = diff.x > 0 ? 1 : (diff.x < 0 ? -1 : 0);
            int dirZ = diff.z > 0 ? 1 : (diff.z < 0 ? -1 : 0);

            Vector3 F_Move = Vector3.zero;
            Vector3 S_Move = Vector3.zero;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
            {
                F_Move = new Vector3(dirX, 0, 0);
                S_Move = new Vector3(0, 0, dirZ);
            }
            else
            {
                F_Move = new Vector3(0, 0, dirZ);
                S_Move = new Vector3(dirX, 0, 0);
            }

            if (F_Move != Vector3.zero && !IsBlocked(transform.position + F_Move)) // First try the primary direction
            {
                transform.rotation = Quaternion.LookRotation(F_Move);
                transform.position += F_Move;
            }
            else if (S_Move != Vector3.zero && !IsBlocked(transform.position + S_Move)) // Second try the secondary direction
            {
                transform.rotation = Quaternion.LookRotation(S_Move);
                transform.position += S_Move;
            }
    }

    void PatrolMove()
    {
        Vector3 moveDir = ccwDirections[dirIndex];

        // Check Wall
        if (!IsBlocked(transform.position + moveDir))
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
            transform.position += moveDir;

            currentPatrolStep++;

            // Rotate Direction
            if (currentPatrolStep >= patrolSteps)
            {
                currentPatrolStep = 0;
                dirIndex = (dirIndex + 1) % ccwDirections.Length;
            }
        }
        else
        {
            // If Blocked, Rotate Direction Immediately
            currentPatrolStep = 0;
            dirIndex = (dirIndex + 1) % ccwDirections.Length;

            transform.rotation = Quaternion.LookRotation(ccwDirections[dirIndex]);
        }
    }

    bool IsBlocked(Vector3 targetPos)
    {
        Physics.SyncTransforms();

        if (targetPos.x < 0f || targetPos.x > MapManager.Instance.currentWidth ||
            targetPos.z < 0f || targetPos.z > MapManager.Instance.currentHeight)
        {
            return true;
        }

        Collider[] hitColliders = Physics.OverlapSphere(targetPos, 0.3f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Wall_B") || 
                hitCollider.CompareTag("Wall_W") ||
                hitCollider.CompareTag("Enemy"))
            {
                return true;
            }
        }
        return false;
    }
}