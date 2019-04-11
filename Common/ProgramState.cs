using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace HD2
{
    public enum ProgramState
    {
        H0_G0_I0, //ko co ng
        H1_G0_I0, //co ng ko tuong tac
        H1_G1_I1, //co ng dang tuong tac
        H1_G1_I0, //co ng moi bo tay xuong

    }

    static class StateControl {
        public static ProgramState prgState;

        public static void SwitchState(ProgramState newState) {
            prgState = newState;
            WriteInteractiveState();
        }
        static void WriteInteractiveState() {
            
            switch (prgState)
            {
                case ProgramState.H0_G0_I0:
                    StateInstruction.writeInteractiveState(false);
                    StateInstruction.writeHumanDetect(false);
                    break;
                case ProgramState.H1_G0_I0:
                    StateInstruction.writeInteractiveState(false);
                    StateInstruction.writeHumanDetect(true);
                    if (ConfigParams.EnableSound)
                    StateInstruction.PlaySound(AssetSource.wavH1G0I0);
                    break;
                case ProgramState.H1_G1_I0:
                    StateInstruction.writeInteractiveState(false);
                    break;
                case ProgramState.H1_G1_I1:
                    StateInstruction.writeInteractiveState(true);
                    if (ConfigParams.EnableSound)
                    StateInstruction.PlaySound(AssetSource.wavH1G1I1);
                    break;
            }
        }
    }
}
