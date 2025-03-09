using Godot;

namespace CrimeChaosAndCatnip.cards;

public partial class CardBase : Resource {

    public virtual bool CanBePlayed(int x, int y) { // TODO: API
        return true;
    }
    
    public virtual void Play(int x, int y) { // TODO: API
    }
    
}
