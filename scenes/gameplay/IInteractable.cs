namespace CrimeChaosAndCatnip;

/// Base for every object on the board, that can be interacted with manually, or automatically when another interactable enters it's field
public interface IInteractable {

	/// Called when another Interactable enters the field this one occupies.
	/// Can be used to e.g. stack objects of the same type or for players/enemies to collect objects
	void OnStacked(IInteractable other);

}
