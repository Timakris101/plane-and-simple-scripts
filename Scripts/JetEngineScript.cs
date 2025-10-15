using UnityEngine;

public class JetEngineScript : EngineScript {
    [SerializeField] protected AnimationCurve enginePowerByAlt;
    [SerializeField] private float thrustKn;
    [SerializeField] private float afterBurner;
    
    public override void setVal(float val) {
        thrustKn = val;
    }

    public override float getThrustNewtons() {
        return enginesOn ? (((PlaneController) vc).getInWEP() ? afterBurner : thrustKn) * 1000f * enginePowerByAlt.Evaluate(transform.position.y) * throttle : 0f;
    }

    public override string getType() {return "thrust";}

    public override float getVal() {return thrustKn;}
    public override float getOverPowerVal() {return afterBurner;}
}
