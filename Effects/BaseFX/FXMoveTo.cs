using UnityEngine;
using FX;

public class FXMoveTo : FXBaseWithEnabled, IFXTriggerable
{
    public GameObject targetLookAt;
    public FXScaledParameter<float> speed = new FXScaledParameter<float>(0.6f, 0, 3f);
    public Vector3 center = new Vector3(0, 0, 0);
    public float sizeX = 1f;
    public float sizeY = 1f;
    public float sizeZ = 1f;

    public enum MoveToType { LERP, SLERP };
    public MoveToType EmoveToType = MoveToType.SLERP;

    private Vector3 target;

    protected override void Awake()
    {
        base.Awake();
        target = transform.position;
    }

    private void LateUpdate()
    {
        if (!fxEnabled.Value) return;

        switch (EmoveToType)
        {
            case MoveToType.SLERP:
                transform.position = Vector3.Slerp(transform.position, target, speed.ScaledValue * Time.deltaTime);
                break;
            case MoveToType.LERP:
                transform.position = Vector3.Lerp(transform.position, target, speed.ScaledValue * Time.deltaTime);
                break;
        }

        if (targetLookAt)
        {
            transform.LookAt(targetLookAt.transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5F);
        Gizmos.DrawWireCube(center, new Vector3(sizeX, sizeY, sizeZ));
    }

    [FXMethod]
    public void FXTrigger()
    {
        target = new Vector3(
        Random.Range(center.x - sizeX * 0.5f, center.x + sizeX * 0.5f),
        Random.Range(center.y - sizeY * 0.5f, center.y + sizeY * 0.5f),
        Random.Range(center.z - sizeZ * 0.5f, center.z + sizeZ * 0.5f)
        );
    }
}
