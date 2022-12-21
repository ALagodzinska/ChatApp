using DataLayer;

namespace ServiceLayer
{
    public class ChatOperations
    {
        private readonly HermesRepository _repository;
        private readonly LoginOperations _loginOperator;

        public ChatOperations(HermesRepository repository, LoginOperations loginOperator)
        {
            _repository = repository;
            _loginOperator = loginOperator;
        }

        public List<Group> GetListOfGroups()
        {
            return _repository.GetListOfGroups();
        }

        public void AddGroup(string groupName)
        {
            var group = new Group() { GroupName = groupName };
            _repository.AddGroup(group);
        }

        public Group? GetGroupByName(string groupName)
        {
            var groupList = _repository.GetListOfGroups();
            var foundGroup = groupList.FirstOrDefault(g => g.GroupName == groupName);
            return foundGroup;
        }
    }
}