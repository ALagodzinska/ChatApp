namespace HermesChatApp.Hubs
{
    public class GroupDictionary
    {
        public Dictionary<string, List<HubUser>> dictionary = new Dictionary<string, List<HubUser>>();

        public void Add(string key, HubUser user)
        {
            if (this.dictionary.ContainsKey(key))
            {
                List<HubUser> list = this.dictionary[key];
                if (!list.Contains(user))
                {
                    list.Add(user);
                }
            }
            else
            {
                List<HubUser> list = new List<HubUser>();
                list.Add(user);
                this.dictionary.Add(key, list);
            }
        }
        public void Remove(string key, HubUser value)
        {
            if (this.dictionary.ContainsKey(key))
            {
                List<HubUser> list = this.dictionary[key];
                var user = list.Find(x => x.Name == value.Name);
                if(user != null)
                {
                    list.Remove(user);
                }                    
                if (list.Count == 0)
                {
                    dictionary.Remove(key);
                }
            }
        }
        public List<HubUser>? GetListOfGroupUsers(string key)
        {
            if (this.dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }
        public List<string>? GetListOfGroups()
        {
            return dictionary.Keys.ToList();
        }

        public void RemoveUserFromGroup(HubUser value)
        {
            var listOfGroups = dictionary.Keys.ToList();
            foreach (var group in listOfGroups)
            {
                if (this.dictionary.ContainsKey(group))
                {
                    List<HubUser> list = this.dictionary[group];
                    var user = list.Find(x => x.Name == value.Name);
                    if (user != null)
                    {
                        list.Remove(user);
                    }
                    if (list.Count == 0)
                    {
                        dictionary.Remove(group);
                    }
                }
            }
           
        }
    }
}
