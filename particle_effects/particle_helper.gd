extends Node3D

func _ready() -> void:
	for child in get_children():
		if child is GPUParticles3D:
			(child as GPUParticles3D).emitting = true
			(child as GPUParticles3D).finished.connect(func()->void: queue_free())
