using UnityEngine;
using FX;

public class FXMoveTo : FXBaseWithEnabled, IFXTriggerable
{
    public GameObject targetLookAt;
    public FXScaledParameter<float> speed = new FXScaledParameter<float>(0.6f, 0, 3f);
    public Vector3 center = new Vector3(0, 0, 0);
    public FXParameter<float> sizeX = new FXParameter<float>(1f);
    public FXParameter<float> sizeY = new FXParameter<float>(1f);
    public FXParameter<float> sizeZ = new FXParameter<float>(1f);

    public enum MoveToType { LERP, SLERP };
    public MoveToType EmoveToType = MoveToType.SLERP;

    private Vector3 target;

    protected override void Awake()
    {
        base.Awake();
        target = transform.position;
    }

    private void Update()
    {
        if (!fxEnabled.Value) return;

        if (targetLookAt)
        {
            transform.LookAt(targetLookAt.transform);
        }

        switch (EmoveToType)
        {
            case MoveToType.SLERP:
                transform.position = Vector3.Slerp(transform.position, target, speed.ScaledValue * Time.deltaTime);
                break;
            case MoveToType.LERP:
                transform.position = Vector3.Lerp(transform.position, target, speed.ScaledValue * Time.deltaTime);
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5F);
        Gizmos.DrawWireCube(center, new Vector3(sizeX.Value, sizeY.Value, sizeZ.Value));
    }

    [FXMethod]
    public void FXTrigger()
    {
        target = new Vector3(
        Random.Range(center.x - sizeX.Value * 0.5f, center.x + sizeX.Value * 0.5f),
        Random.Range(center.y - sizeY.Value * 0.5f, center.y + sizeY.Value * 0.5f),
        Random.Range(center.z - sizeZ.Value * 0.5f, center.z + sizeZ.Value * 0.5f)
        );
    }
}
