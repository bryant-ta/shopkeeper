using TriInspector;
using UnityEngine;

public class UpgradeManager : MonoBehaviour {
    
}

public abstract class Upgrade {
    [field: SerializeField, ReadOnly] public UpgradeID ID { get; private set; }
    public string Name { get; private set; }

    public void Execute() {
        
    }
}

/// last time: debating best way to structure upgrades
/// - definitely using Execute() -> actively call function/reference target to do particular upgrade
/// - abstract class vs. scriptable obj -> consider setting strings for upgrade name/description, but also
///   need custom code defined Execute() somewhere
///     - can do enum to select correct upgrade effect and put in SO... but why is SO advantagous? just bc can set strings?
///       can't i do that in abstract class...? prob have to be strings written out in script... is thta even bad?
///       also just using class implementations can allow more flexibility in upgrade parameters, SO would need a child SO to define
///       upgrade specific variables
///     - maybe another way? needs research, leaning towards abstract, dont see why strings in script is bad if never change anyways

public class SO_Upgrade {
    public UpgradeID ID { get; private set; }
    public string Name { get; private set; }

    public void Execute() {
        
    }
}

public enum UpgradeID {
    ZoomView = 1,
    RaiseCarryLimit = 2,
}