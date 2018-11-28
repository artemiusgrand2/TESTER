using System;
using System.Collections.Generic;
using System.Text;

namespace sdm.diagnostic_section_model.client_impulses.requests
{
    unsafe class ImpulsesAnswer
    {
        /// <summary>
        /// Заголовок ответа.
        /// </summary>
        public ImpulsesAnswerHeader Header;

        /// <summary>
        /// Импульсы ТС.
        /// </summary>
        public byte[] TsImpulses;

        /// <summary>
        /// Импульсы ТУ.
        /// </summary>
        public byte[] TuImpulses;
    }
}
