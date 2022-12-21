namespace HermesChatApp.Hubs
{
    public class MessageDictionary
    {

        public Dictionary<string, List<HubMessage>> dictionary = new Dictionary<string, List<HubMessage>>();

        public void Add(string key, HubMessage value)
        {
            if (this.dictionary.ContainsKey(key))
            {
                List<HubMessage> list = this.dictionary[key];
                if (list.Count < 10)
                {
                    list.Add(value);
                }
                else
                {
                    list.RemoveAt(0);
                    list.Add(value);
                }
            }
            else
            {
                List<HubMessage> list = new List<HubMessage>();
                list.Add(value);
                this.dictionary.Add(key, list);
            }
        }
        public List<HubMessage>? GetLastMessageList(string key)
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
    }
}
