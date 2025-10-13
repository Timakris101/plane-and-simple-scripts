using UnityEngine;

public class PistonEngineScript : EngineScript {
    [SerializeField] protected AnimationCurve enginePowerByAlt;
    [SerializeField] protected float powerHp;
    [SerializeField] private float wepHp;
    private float propEff = .7f;

    public override void setVal(float val) {
        powerHp = val;
    }

    public override float getThrustNewtons(float speed) {
        bool anyPropellers = false;
        for (int i = 0; i < transform.parent.childCount; i++) {
            if (transform.parent.GetChild(i).GetComponent<PropellerScript>() != null) {
                anyPropellers = true;
                break;
            }
        }
        if (!anyPropellers) return 0;
        return (((PlaneController) vc).getInWEP() ? wepHp : powerHp) / Mathf.Max(30f, speed) * 745.7f * enginePowerByAlt.Evaluate(transform.position.y) * propEff;
    }

    public override string getType() {return "power";}

    public override float getVal() {return powerHp;}
    public override float getOverPowerVal() {return wepHp;}
}
