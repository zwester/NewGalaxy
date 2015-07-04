using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Data.SQLite;

namespace WindowsGame1
{
    public class Card
    {
        //use id for testing equality
        public int id;
        public bool selected;
        public Texture2D texture;
        public int x, y;
        public float z;
        public Rectangle size;
        public bool mouseOver;
        public Player owner;
        public string name;
        public int cost;
        public int points;
        public bool world;
        public bool startingWorld;
        public bool military;
        public bool rebel;
        public string windfall;
        public string explore, develop, settle, trade, consume, produce;
        public bool hasGood;
        //TODO: can this be an actual color?
        public string produceColor;
        public int tempMilitary;
        public bool freeWorld;
        public int tradeThis;
        public int produceThisDraw;
        public bool hasBorder;
        public Section curSection;
        //This can be overridden if Section is not selectable
        //TODO: Decide if I should implement this
        public bool isSelectable;
        public ConsumePowers consumePower = null;
        public bool consumeAll;


        public Card(Section sect)
        {
            id = -1;
            selected = false;
            mouseOver = false;
            curSection = sect;
            tempMilitary = 0;
        }

        public Card(Card c)
        {
            this.id = c.id;
            selected = false;
            mouseOver = false;
            texture = c.texture;
            z = x = y = 0;
            size = new Rectangle(x, y, 0, 0);
            owner = null;
            this.name = c.name;
            this.cost = c.cost;
            this.points = c.points; this.world = c.world; this.startingWorld = c.startingWorld; this.military = c.military; this.windfall = c.windfall;
            this.explore = c.explore; this.develop = c.develop; this.settle = c.settle; this.trade = c.trade; this.consume = c.consume; this.produce = c.produce;
            this.produceColor = c.produceColor; this.tempMilitary = c.tempMilitary; this.freeWorld = c.freeWorld; this.tradeThis = c.tradeThis; this.produceThisDraw = c.produceThisDraw;
        }

        public Card(int id,string name,int cost,int points,bool world,bool startingWorld,bool military,string windfall,string explore,string develop,string settle,string trade,string consume,string produce)
        {
            this.id = id;
            texture = null;
            selected = false;
            z = x = y = 0;
            size=new Rectangle(x,y,0,0);
            mouseOver = false;
            owner = null;
            this.name = name;
            this.cost = cost;
            this.points = points; this.world = world; this.startingWorld = startingWorld; this.military = military; this.windfall = windfall;
            this.explore = explore; this.develop = develop; this.settle = settle; this.trade = trade; this.consume = consume; this.produce = produce;
            tempMilitary = 0;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Card return false.
            Card c = obj as Card;
            if ((System.Object)c == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this.id == c.id;
        }

        public static int dropTable(String tblName)
        {
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=CardDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
            
            string sql = "DROP TABLE "+tblName;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            try { return command.ExecuteNonQuery(); }
            catch (SQLiteException e) { Console.WriteLine("Error dropping");  return 1; };
        }

        public static int makeTable(string datasource,string tblName)
        {
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source="+datasource+".sqlite;Version=3;");
            m_dbConnection.Open();

            string sql = "CREATE TABLE "+tblName +" (id int PRIMARY KEY, name varchar(32),cost int,points int,world bit,startingWorld bit,militaryWorld bit,windfall varchar(6),explore varchar(32),develop varchar(32),settle varchar(32),trade varchar(32),consume varchar(32),produce varchar(32))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            return command.ExecuteNonQuery();
        }

        public static void buildSQLDeck(string datasource, string tblName)
        {
            //SQLiteConnection.CreateFile("CardDatabase.sqlite");
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source="+datasource+".sqlite;Version=3;");
            m_dbConnection.Open();

                        //name,cost,points,world?,startingworld?,militaryworld?,windfall color,explore bonus,develop bonus,settle bonus,trade bonus,consume bonus,produce bonus
            string sql = ",'old earth',3,2,1,1,0,'','','','','draw:1','any:1:2:0:1','')>" +//0
                        ",'epsilon eridani',2,1,1,1,0,'','','','military:1','','any:1:1:1:1','')>" +//1
                        ",'alpha centauri',2,0,1,1,0,'brown','','','specific:C:brown:1,specific:M:brown:1','','','')>" +
                        ",'new sparta',2,1,1,1,1,'','','','military:2','','','')>" +
                        ",'earth''s lost colony',2,1,1,1,0,'','','','','','any:1:1:0:1','1blue')>" +
                        ",'rebel fuel cache',1,1,1,0,1,'brown','','','','','','')>" +//5
                        ",'public works',1,1,0,0,0,'','','after:1','','','any:1:1:0:1','')>" +
                        ",'gem world',2,1,1,0,0,'','','','payForMilitary','','','1blue and draw 1card')>" +
                        ",'colony ship',2,1,0,0,0,'','','','freeWorld','','','')>" +
                        ",'expedition force',1,1,0,0,0,'','draw:1','','military:1','','','')>" +
                        ",'rebel miners',2,1,1,0,1,'','','','','','','1brown')>" +//10
                        ",'mining robots',2,1,0,0,0,'','','','specific:C:brown:1','','','produce on a brown windfall')>" +
                        ",'comet zone',3,2,1,0,0,'','','','','','','1brown and draw 1card')>" +
                        ",'export duties',1,1,0,0,0,'','','','','draw:1','','')>" +
                        ",'new military tactics',1,1,0,0,0,'','','','temp:3','','','')>" +
                        ",'former penal colony',2,1,1,0,1,'blue','','','military:1','','','')>" +//15
                        ",'malevolent life forms',4,2,1,0,1,'','draw:1','','','','','1green')>" +
                        ",'contact specialist',1,1,0,0,0,'','','','military:-1,payForMilitary','','','')>" +
                        ",'avian uplift race',2,2,1,0,1,'green','','','','','','')>" +
                        ",'spice world',2,1,1,0,0,'','','','','specific:blue:2,'','1blue')>" +
                        ",'space marines',2,1,0,0,0,'','','','miltiary:2','','','')>" +//20
                        ",'pilgrimage world',0,2,1,0,0,'','','','','','all','')>" +
                        ",'refugee world',0,1,1,0,0,'blue','','','military:-1','','','')>" +
                        ",'artist colony',1,1,1,0,0,'','','','','','','1blue')>" +
                        ",'contact specialist',1,1,0,0,0,'','','','military:-1,payForMilitary','','','')>" +
                        ",'destroyed world',1,0,1,0,0,'brown','','','','','','')>" +//25
                        ",'empath world',1,1,1,0,0,'green','','','military:-1','','','')>" +
                        ",'expanding colony',1,1,1,0,0,'','','','','','any:1:1:0:1','produce on a blue windfall')>" +
                        ",'expedition force',1,1,0,0,0,'','draw:1','','military:1','','','')>" +
                        ",'export duties',1,1,0,0,0,'','','','','draw:1','','')>" +
                        ",'gambling world',1,1,1,0,0,'','','','','','lucky','')>" +//30
                        ",'investment credits',1,1,0,0,0,'','','reduce:1','','','','')>" +
                        ",'investment credits',1,1,0,0,0,'','','reduce:1','','','','')>" +
                        ",'new military tactics',1,1,0,0,0,'','','','temp:3','','','')>" +
                        ",'new survivalists',1,1,1,0,0,'','','','','',':blue:1:1:1:0','1blue')>" +
                        ",'outlaw world',1,1,1,0,1,'','','','','','any:1:1:1:1','')>" +//35
                        ",'public works',1,1,0,0,0,'','','after:1','','','any:1:1:0:1','')>" +
                        ",'runaway robots',1,1,1,0,1,'brown','','','','','','draw 1 card if produced on this world')>" +
                        ",'secluded world',1,1,1,0,0,'','','','','','any:1:1:1:0','1blue')>" +
                        ",'star nomad lair',1,1,1,0,0,'blue','draw:1','','specific:blue:2','','','')>" +
                        ",'the last of the uplift gnarssh',1,0,1,0,0,'green','','','','','','')>" +//40
                        ",'alien robot sentry',2,2,1,0,1,'yellow','','','','','','')>" +
                        ",'aquatic uplift race',2,2,1,0,1,'green','','','','','','')>" +
                        ",'asteroid belt',2,1,1,0,0,'brown','','','','','','')>" +
                        ",'colony ship',2,1,0,0,0,'','','','freeWorld','','','')>" +
                        ",'deficit spending',2,1,0,0,0,'','','','','','card:1:2:0:1','')>" +//45
                        ",'deficit spending',2,1,0,0,0,'','','','','','card:1:2:0:1','')>" +
                        ",'galactic engineers',2,1,1,0,0,'','','','','draw an extra card','','produce 1 of any windfall')>" +
                        ",'genetics lab',2,1,0,0,0,'','','','','specific:green:1','','produce 1green windfall')>" +
                        ",'genetics lab',2,1,0,0,0,'','','','','specific:green:1','','produce 1green windfall')>" +
                        ",'interstellar bank',2,1,0,0,0,'','','draw:1','','','','')>" +//50
                        ",'interstellar bank',2,1,0,0,0,'','','draw:1','','','','')>" +
                        ",'mining robots',2,1,0,0,0,'','','','specific:C:brown:1','','','produce on a brown windfall')>" +
                        ",'new vinland',2,1,1,0,0,'','','','','','any:1:1:2:0','1blue')>" +
                        ",'pre-sentient race',2,1,1,0,0,'green','','','','','','')>" +
                        ",'radioactive world',2,1,1,0,0,'brown','','','','','','')>" +//55
                        ",'reptilian uplift race',2,2,1,0,1,'green','','','','','','')>" +
                        ",'space marines',2,1,0,0,0,'','','','military:2','','','')>" +
                        ",'space port',2,1,1,0,0,'','','','','specific:brown:2','','1blue')>" +
                        ",'alien rosetta stone world',3,3,1,0,0,'','','','specific:C:yellow:2,specific:M:yellow:2','','','produce 1yellow windfall')>" +
                        ",'bio-hazard mining world',3,2,1,0,0,'','','','','specific:green:2','','1brown')>" + //60
                        ",'black market trading world',3,2,1,0,0,'','','','','','any:1:1:trade:0','')>" +
                        ",'blaster gem mines',3,2,1,0,0,'brown','','','military:1','','','')>" +
                        ",'galactic resort',3,2,1,0,0,'','','','','','any:1:1:1:1','')>" +
                        ",'mining conglomerate',3,2,0,0,0,'','','','','specific:brown:1','brown:1:2:0:1','draw 2 cards if produced the most brown')>" +
                        ",'mining conglomerate',3,2,0,0,0,'','','','','specific:brown:1','brown:1:2:0:1','draw 2 cards if produced the most brown')>" + //65
                        ",'mining world',3,2,1,0,0,'','','','','','','1brown and draw 1card')>" +
                        ",'pirate world',3,2,1,0,1,'blue','','','','this:3','','')>" +
                        ",'plague world',3,0,1,0,0,'','','','','','green:1:1:1:1:','1green')>" +
                        ",'prosperous world',3,2,1,0,0,'','','','','','any:1:1:0:1','1blue')>" +
                        ",'rebel underground',3,4,1,0,1,'','','','','','','1card')>" + //70
                        ",'rebel warrior race',3,2,1,0,1,'green','','','military:1','','','')>" +
                        ",'terraforming robots',3,2,0,0,0,'','','','after:1','','brown:1:1:1:1','')>" +
                        ",'terraforming robots',3,2,0,0,0,'','','','after:1','','brown:1:1:1:1','')>" +
                        ",'alien robot scout ship',4,2,1,0,1,'yellow','','','military:1','','','')" +
                        ",'deserted alien outpost',4,3,1,0,0,'yellow','','','','','','')" + //75
                        ",'distant world',4,3,1,0,0,'','','','','specific:blue:3','','1green')" +
                        ",'diversified economy',4,2,0,0,0,'','','','','','different:3:1:0:3','draw 1 card for each kind of good')" +
                        ",'diversified economy',4,2,0,0,0,'','','','','','different:3:1:0:3','draw 1 card for each kind of good')" +
                        ",'drop ships',4,2,0,0,0,'','','','military:3','','','')" +
                        ",'drop ships',4,2,0,0,0,'','','','military:3','','','')" + //80
                        ",'imperium armaments world',4,2,1,0,0,'','','','military:1','','','1brown')" +
                        ",'merchant world',4,2,1,0,0,'','','','','draw:2','card:1:2:0:1','')" +
                        ",'replicant robots',4,2,0,0,0,'','','','reduce:2','','','')" +
                        ",'replicant robots',4,2,0,0,0,'','','','reduce:2','','','')" +
                        ",'research labs',4,2,0,0,0,'keep:1','','','','','green:1:1:0:1','draw 1card for each yellow produced')" + //85
                        ",'research labs',4,2,0,0,0,'keep:1','','','','','green:1:1:0:1','draw 1card for each yellow produced')" +
                        ",'tourist world',4,2,0,0,0,'','','','','','any:2:1:0:3','')" +
                        ",'consumer markets',5,3,0,0,0,'','','','','','blue:1:3:0:1','draw 1card for each blue produced')" +
                        ",'consumer markets',5,3,0,0,0,'','','','','','blue:1:3:0:1','draw 1card for each blue produced')" +
                        ",'deserted alien colony',5,4,1,0,0,'yellow','','','','','','')" + //90
                        ",'galactic trendsetters',5,3,1,0,0,'','','','','','any:1:1:0:2','')" +
                        ",'lost alien warship',5,3,1,0,1,'yellow','','','military:2','','','')" +
                        ",'lost species ark world',5,3,1,0,0,'','','','','','','1green then draw +2cards')" +
                        ",'new earth',5,3,1,0,0,'','','','','','any:1:1:1:1','1brown')" +
                        ",'rebel outpost',5,5,1,0,1,'','','','military:1','','','')" + //95
                        ",'terraformed world',5,5,1,0,0,'','','','','','any:1:1:0:1','')";

            for(int i=0;i<sql.Split('>').Length;i++)
            {
                try{
                string sqlCommand = "insert into standard_cards values (" + i +sql.Split('>')[i];
                SQLiteCommand command = new SQLiteCommand(sqlCommand, m_dbConnection);
                command.ExecuteNonQuery();
                }
                catch (SQLiteException e) { Console.WriteLine("ERROR adding: "+i); }
            }
        }

        public static void readDeck(string datasource, string tblName, Queue<Card> deck)
        {
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=" + datasource + ".sqlite;Version=3;");
            m_dbConnection.Open();

            string sql = "select * from "+tblName;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                deck.Enqueue(new Card(Convert.ToInt16(reader["id"]),reader["name"].ToString(), Convert.ToInt16(reader["cost"]), Convert.ToInt16(reader["points"]),
                    Convert.ToBoolean(reader["world"]), Convert.ToBoolean(reader["startingworld"]), Convert.ToBoolean(reader["militaryWorld"]),
                    reader["windfall"].ToString(), reader["explore"].ToString(), reader["develop"].ToString(), reader["settle"].ToString(),
                    reader["trade"].ToString(), reader["consume"].ToString(), reader["produce"].ToString()));
                //Console.WriteLine("Id:" + reader["id"] + " Name: " + reader["name"] + " Cost: " + reader["cost"] + " Points: " + reader["points"]);
            }
        }

        public void parseSettle(string str)
        {
            //freeWorld and tempMilitary are done for one round only so do them per card
            string[] split = str.Split(':');
            if (split[0].Equals("freeWorld")) freeWorld = true;
            else if (split[0].Equals("temp")) tempMilitary += Int16.Parse(split[1]);
        }

        public void parseTrade(string str)
        {
            string[] split = str.Split(':');
            if (split[0].Equals("this")) tradeThis += Int16.Parse(split[1]);
        }

        public void parseConsume(string str)
        {
            string[] split = str.Split(':');
            
        }

        public void select(bool isSelected)
        {
            this.selected = isSelected;
            curSection.totalSelected += (2 * Convert.ToInt16(selected) - 1);//if selected add 1 total,if deselect subtract 1 from total
            if (selected)
                curSection.lastCardSelected = curSection.FindIndex(xs => xs.Equals(this));
            else
                curSection.lastCardSelected = -1;
        }

        public void invertSelect()
        {
            this.selected = !this.selected;
            curSection.totalSelected += (2 * Convert.ToInt16(selected) - 1);//if selected add 1 total,if deselect subtract 1 from total
            if (selected)
                curSection.lastCardSelected = curSection.FindIndex(xs => xs.Equals(this));
            else
                curSection.lastCardSelected = -1;
        }
    }

}
