# Simulation-Based Reinforcement Learning for Airborne Trash Interception

This project uses **Unity ML-Agents** and **Proximal Policy Optimization (PPO)** to train a mobile trash bin to track and intercept airborne trash in a simulated arena.

Instead of training directly on a real robot (which is slow, risky, and expensive), the policy is learned entirely in simulation and exported as an **ONNX** model for inference inside Unity.

---

## Requirements

- **Unity**: e.g. 202x.x.x (any version that supports the ML-Agents package you use)
- **Unity ML-Agents package** in the project
- **Python 3.10**  
- Recommended: **Conda** (Anaconda or Miniconda)
- GPU with CUDA support (optional but nice to have)

Python packages:

```bash
pip install mlagents torch tensorboard


### Training

1. Create and activate a separate Python environment.
2. Install the required Python packages (e.g. `mlagents`, `torch`, `tensorboard`).
3. In a terminal, navigate to the **ml-agents root folder**.
4. Start training:

```bash
python -m mlagents.trainers.learn config/<your_config>.yaml \
    --run-id=<What_ever_ID_Name_you_want \
    --torch-device=cuda /// Or use "--torch-device=cpu" if you dont have a GPU 
