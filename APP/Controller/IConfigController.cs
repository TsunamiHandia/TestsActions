using System;

namespace APP.Controller {
    public interface IConfigController {
        public string GetValue(Enum id, Guid? moduleId = null);
    }
}