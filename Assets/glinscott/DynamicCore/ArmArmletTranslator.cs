namespace GarboDev
{
    using System;
    using System.Collections.Generic;

    public class ArmArmletTranslator
    {
        private Arm7Processor parent;
        private Memory memory;
        private uint[] registers;

        public ArmArmletTranslator(Arm7Processor parent, Memory memory)
        {
            this.parent = parent;
            this.memory = memory;
            this.registers = this.parent.Registers;
        }


    }
}
