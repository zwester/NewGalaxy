using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Data.SQLite;

namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //int curPhase = 0;

        string dataSource = "CardDatabase", tblName = "standard_cards";
        public static Queue<Card> deck = new Queue<Card>();
        public static Queue<Player> players = new Queue<Player>();
        public static int cardWidth, cardHeight;
        //Some constants---------------
        public static int MaxY, MaxX;
        int numPlayers,textureHeight,textureWidth;
        public static float x0, y0;
        public static int y1, y2;
        public static float selectOffsetY = 0.15F;
        public static float mouseOverScale = 1.15F;
        public static float goodsOffset = 0.1F;
        public static float borderSize = 0.03F;
        public static float yBufferPercent = 0.1F;
        float bufferSum;
        float cardScale;
        public static float mouseOverOffset = (mouseOverScale - 1) / 2F;
        public static int maxHandSize = 10;
        public static string selectionPhaseMessage = "Select phase: Select 1 action to perform.";
        public static string explorePhaseMessage = "Explore phase: Select 1 cards to keep.";
        public static string developPhaseMessage = "Develop phase: Choose one building from your hand. Then select cards to pay for it.";
        public static string settlePhaseMessage = "Settle phase: Choose one world from your hand. Then select cards to pay for it.";
        //--------
        int curSection,prevSection;
        public static String currentPhase = "Start";
        public string nextPhaseStr = "";
        public static bool skipPhase = false; 
        List<bool> phasesSelected = new List<bool>(5);
        public static bool confirmClick = false;
        public static int sectionClicked = -1;
        //Other players cards played
        public static List<Section> allSections = new List<Section>(8);
        List<Card> tableauOne = new List<Card>();
        List<Card> tableauTwo = new List<Card>();
        List<Card> tableauThree = new List<Card>();
        List<Card> allActions = new List<Card>();
        public Section myActions = null;
        List<Card> myPhase = new List<Card>();
        public bool phaseComplete = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            myActions = new Section(7, this);

            //each player discards 2 of their starting cards
            
            //phase select
            //explore

            //explorePhase(new Player(1));
            //develop
            //settle
            //trade
            //consume
            //produce
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>

        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;

            //Set some graphics constants
            bufferSum = yBufferPercent + mouseOverScale + selectOffsetY;
            MaxY = graphics.GraphicsDevice.Viewport.Height;
            MaxX = graphics.GraphicsDevice.Viewport.Width;
            x0 = MaxX / 2;
            y0 = MaxY / 4; y1 = MaxY / 2; y2 = (int) y0 * 3;

            for (int i = 0; i < phasesSelected.Capacity; i++)
            {
                phasesSelected.Add(false);
            }

            //build deck
            
            SQLiteConnection.CreateFile("CardDatabase.sqlite");
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=CardDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
            Card.dropTable(tblName);
            Card.makeTable(dataSource,tblName);
            Card.buildSQLDeck(dataSource,tblName);
             
            Card.readDeck(dataSource, tblName, deck);
            //Card.shuffleDeck(deck);
            numPlayers = 2;
            for (int i = 0; i < allSections.Capacity; i++)
            {
                allSections.Add(new Section(i, 8, this));
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        Texture2D goodsTexture, borderTexture;
        public static float xBuffer, yBuffer;
        private SpriteFont errorFont;
        public static string errorMessage = "";
        public static string phaseMessage = "Select 1 action to perform.";

        protected override void LoadContent()
        {
            
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            errorFont = Content.Load<SpriteFont>("Error");
            foreach (Card c in deck)
            {
                c.texture = Content.Load<Texture2D>("cougar");
                c.size = new Rectangle(c.texture.Bounds.X, c.texture.Bounds.Y, (int)(c.texture.Bounds.Width*.1), (int)(c.texture.Bounds.Height*.1));
                //c.size = c.texture.Bounds;
            }

            textureWidth = deck.Peek().texture.Width;
            textureHeight = deck.Peek().texture.Height;

            cardScale = ((MaxY / 4) / bufferSum) / textureHeight;
            cardHeight = (int)(textureHeight * cardScale);
            cardWidth = (int)(textureWidth * cardScale);
            xBuffer = cardWidth * 0.4F;
            yBuffer = cardHeight * 0.2F;
         
            goodsTexture = new Texture2D(GraphicsDevice, 1, 1);
            goodsTexture.SetData(new Color[] { Color.Yellow });

            borderTexture = new Texture2D(GraphicsDevice, 1, 1);
            borderTexture.SetData(new Color[] { Color.Black });
         
            for (int i = 0; i < 1; i++)//for numPlayers
            {
                players.Enqueue(new Player(i, this));
                /*for (int j = 0; j < 2; j++)//each player starts with 6 cards
                {
                    players.ElementAt<Player>(i).addCardToHand(deck.Dequeue());
                    players.ElementAt<Player>(i).tableau.Add(deck.Dequeue());
                }*/
            }
            players.Peek().hand = allSections[6];
            players.Peek().tableau = allSections[5];

            //Give 6 starting cards to each player- SHOULD HAVE TO DISCARD 2
            for (int i = 0; i < 7; i++)
            {
                players.Peek().hand.AddCard(deck.Dequeue());
            }
            Console.WriteLine(deck.Peek().settle+": settle");
            //PLAYER STARTS WITH STARTING WORLD
            players.Peek().addCardToTableau(deck.Dequeue());
            players.Peek().addCardToTableau(deck.Dequeue());
            players.Peek().addCardToTableau(deck.Dequeue());
            players.Peek().addCardToTableau(deck.Dequeue());
            players.Peek().tableau.maxSize = 12;
            allSections[0].AddCard(deck.Dequeue());
            allSections[0].maxSize = 12;
            allSections[1].AddCard(deck.Dequeue());
            allSections[1].maxSize = 12;
            allSections[2].AddCard(deck.Dequeue());
            allSections[2].maxSize = 12;
            //TODO:setup action deck
            for (int i = 0; i < 7; i++)
            {
                myActions.AddCard(deck.Dequeue());
            }
            //4th section starts off as myactions to choose from
            allSections[4] = new Section(4, myActions, this);
            //This is going to be info/confirm button section
            allSections[7].AddCard(deck.Dequeue());
            nextPhase();
            //setupSelectionPhase(players.Peek());
            //Make actionSelect clickable
            //allSections[4].isSelectable = true;
            //allSections[7].isSelectable = true;
            /*for (int i = 0; i < 8; i++)
            {
                tableauOne.Add(deck.Dequeue()); //player 1 board
                tableauTwo.Add(deck.Dequeue()); //player 2 board
                tableauThree.Add(deck.Dequeue()); //player 3 board
                myPhase.Add(deck.Dequeue()); //cards needed for whatever phase I,m currently doing
                myActions.Add(deck.Dequeue()); //my phase cards to choose from at start of each round...could this be combined with myPhase section?
                allActions.Add(deck.Dequeue()); //See all the actions that all players chose this round
            }*/

            initSections();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        //checks if mouse button has been released since it was last pressed
        bool hasBeenReleased;
        //ratio for how much cards are overlapping with each other
        float handOverlap = 1;

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (MaxX != graphics.GraphicsDevice.Viewport.Width || MaxY != graphics.GraphicsDevice.Viewport.Height) onWindowResize();

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Released)
                hasBeenReleased = true;
            if (ms.X < 0 | ms.X >= MaxX | ms.Y < 0 | ms.Y >= MaxY | (!this.IsActive)) { allSections[curSection].resetSection(); }
            else
            {
                prevSection = curSection;
                curSection = Convert.ToInt16(ms.X > x0) + 2 * (int)(ms.Y / y0);
                if (prevSection != curSection) allSections[prevSection].resetSection();
                allSections[curSection].updateSection(ms, hasBeenReleased);
            }
            if (ms.LeftButton == ButtonState.Pressed )
                hasBeenReleased = false;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the sprite.
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            spriteBatch.DrawString(errorFont, phaseMessage+"\n"+errorMessage, new Vector2(500, 300), Color.Black, 0f, new Vector2(0,0),0.75f, SpriteEffects.None, 1f);
            foreach (Section sec in allSections)
            {
                Color secColor = Color.White; 
                if (!sec.isSelectable) secColor = Color.Gray;
                spriteBatch.DrawString(errorFont, sec.sectionNumber.ToString(), new Vector2(sec.xShift + xBuffer, sec.yShift-yBuffer-cardHeight-(2.5F*cardHeight*goodsOffset)), Color.Black, 0f, new Vector2(0, 0), 0.75f, SpriteEffects.None, 1f);
                foreach (Card c in sec)
                {
                    c.hasGood = true;
                    if (c.hasGood) spriteBatch.Draw(goodsTexture, new Rectangle(c.x, c.y - (int)(c.size.Height * goodsOffset), (int)(0.9F * sec.handOverlap * c.size.Width), (int)(c.size.Height * goodsOffset)), null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0);
                    if (c.hasBorder) spriteBatch.Draw(borderTexture, new Rectangle(c.x - (int)(c.size.Width * borderSize), c.y - (int)(c.size.Height * borderSize), (int)((1 + 2 * borderSize) * c.size.Width), (int)(c.size.Height * (1 + 2 * borderSize))), null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, c.z - .001F);
                    spriteBatch.Draw(c.texture, new Vector2(c.x, c.y), null, secColor, 0, Vector2.Zero, (1.0F * c.size.Width / cardWidth) * cardScale, SpriteEffects.None, c.z);
                }
            }
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void initSections()
        {
            foreach (Section sec in allSections)
            {
                sec.calcHandOverlap();
                sec.calcXYShift(sec.sectionNumber);
                sec.resetSection();
            }
        }

        public void nextPhase()
        {
            phaseMessage = "";
            if (currentPhase.Equals("Wait"))
            {
                errorMessage = "";
                skipPhase = false;
                currentPhase = nextPhaseStr;
                nextPhaseStr = "";
                chooseSetup(currentPhase);
                return;
            }
            if (currentPhase.Equals("Start"))
            {
                currentPhase = "Selection";
            }
            else if(currentPhase.Equals("Selection"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Explore";
            }
            else if (currentPhase.Equals("Explore"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Develop";
            }
            else if (currentPhase.Equals("Develop"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Settle";
            }
            else if (currentPhase.Equals("Settle"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Trade";
            }
            else if (currentPhase.Equals("Trade"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Consume";
            }
            else if (currentPhase.Equals("Consume"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Produce";
            }
            else if (currentPhase.Equals("Produce"))
            {
                currentPhase = "Wait";
                nextPhaseStr = "Upkeep";
            }
            else if (currentPhase.Equals("Upkeep"))
            {
                appendErrorMessage("DONE", false);
                //currentPhase = "Selection"; initSections();
            }
            //If we skipped this phase, then there will be no text at end of phase so don't need to Wait
            if (skipPhase)
            {
                currentPhase = nextPhaseStr;
                nextPhaseStr = "";
                skipPhase = false;
            }
            chooseSetup(currentPhase);
        }

        public void chooseSetup(String phase)
        {
            switch (phase)
            {
                case "Selection":
                    setupSelectionPhase(players.Peek());
                    break;
                case "Explore":
                    setupExplorePhase(players.Peek());
                    break;
                case "Develop":
                    setupDevelopPhase(players.Peek());
                    break;
                case "Settle":
                    setupSettlePhase(players.Peek());
                    break;
                case "Trade":
                    setupTradePhase(players.Peek());
                    break;
                case "Consume":
                    setupConsumePhase(players.Peek());
                    break;
                case "Wait":
                    setupWaitPhase(players.Peek());
                    break;
                default:
                    break;
            }
        }

        public void setupWaitPhase(Player player){
            //Need clear out other sections
            allSections[4] = new Section(4, 0, this);
            allSections[4].minSelectable = 0;
            allSections[4].maxSelectable = 0;
            allSections[6].isSelectable = false;
            allSections[5].isSelectable = false;
        }

        public void setupSelectionPhase(Player player)
        {
            //Need user to select exactly 1 card before this is ok to hit
            allSections[7].isSelectable = false;
            allSections[4].isSelectable = true;
            allSections[4].minSelectable = 1;
            allSections[4].maxSelectable = 1;
        }

        public void selectPhase(Card c, int index, int sectionNumber)
        {
            //We're selecting or deselecting a candidate action card
            if (sectionNumber == 4)
            {
                c.invertSelect();
                if (allSections[4].totalSelected > 0)
                    allSections[7].isSelectable = true;
                else
                    allSections[7].isSelectable = false;
                return;
            }
            //Only other thing you can click in this phase is the confirm button
            else if (sectionNumber != 7 || index != 0) return;
            //not enough were selected so display error message and quit
            if (allSections[4].totalSelected < allSections[4].minSelectable)
            {
                string message = "You must select at least " + allSections[4].minSelectable + " cards.";
                appendErrorMessage(message,true);
                return;
            }
            //find card selected
            switch (allSections[4].lastCardSelected)
            {
                case 0:
                    players.Peek().phaseSelected = "I";
                    phasesSelected[0] = true;
                    break;
                case 1:
                    players.Peek().phaseSelected = "I:2";
                    phasesSelected[0] = true;
                    break;
                case 2:
                    players.Peek().phaseSelected = "II";
                    phasesSelected[1] = true;
                    break;
                case 3:
                    players.Peek().phaseSelected = "III";
                    phasesSelected[2] = true;
                    break;
                case 4:
                    players.Peek().phaseSelected = "$";
                    phasesSelected[3] = true;
                    break;
                case 5:
                    players.Peek().phaseSelected = "IV";
                    phasesSelected[3] = true;
                    break;
                case 6:
                    players.Peek().phaseSelected = "V";
                    phasesSelected[4] = true;
                    break;
                default:
                    break;
            }
            allSections[4].isSelectable = false;
            string mess = "You have selected " + allSections[4].lastCardSelected;
            appendErrorMessage(mess, true);
            nextPhase();
        }

        public void setupExplorePhase(Player player)
        {
            if (!phasesSelected[0]) { phaseMessage = "Explore phase not chosen. Moving to next phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
            if (player.phaseSelected.Equals("I"))
            {
                int drawCards = 3 + player.exploreDraw;
                int keepCards = 2 + player.exploreKeep;
                //draw 3, discard 1
                allSections[4] = new Section(4, drawCards, this);
                allSections[4].maxSelectable = keepCards;//how many can be selected to keep
                allSections[4].minSelectable = keepCards;//how many can be selected to keep
                
            }
            else if (player.phaseSelected.Equals("I:2"))
            {
                int drawCards = 7 + player.exploreDraw;
                int keepCards = 1 + player.exploreKeep;
                //draw 7,discard 6
                allSections[4] = new Section(4, drawCards, this);
                allSections[4].maxSelectable = keepCards;//how many can be selected to keep
                allSections[4].minSelectable = keepCards;//how many can be selected to keep
            }
            else
            {
                int drawCards=2 + player.exploreDraw;
                int keepCards=1 + player.exploreKeep;
                //draw 2,discard 1
                allSections[4] = new Section(4, drawCards, this);
                allSections[4].maxSelectable = keepCards;//how many can be selected to keep
                allSections[4].minSelectable = keepCards;//how many can be selected to keep
            }
            //populate section with cards from deck
            for (int i = 0; i < allSections[4].Capacity; i++)
            {
                allSections[4].AddCard(deck.Dequeue());
            }
            explorePhaseMessage = "Explore phase: Select " + allSections[4].minSelectable + " cards to keep.";
            phaseMessage = explorePhaseMessage;
            allSections[4].isSelectable = true;
            allSections[4].resetSection();
            allSections[7].isSelectable = false;
        }

        public void explorePhase(Card card, int index, int sectionNumber)
        {
            //We're selecting or deselecting a candidate cards to add to our hand
            if (sectionNumber == 4)
            {
                card.invertSelect();
                if (allSections[4].totalSelected < allSections[4].minSelectable)
                    allSections[7].isSelectable = false;
                else
                    allSections[7].isSelectable = true;
                return;
            }
            if (sectionNumber != 7 || index != 0) return;
            //No one picked this phase. wait for confirm button to advance
            if (skipPhase) { nextPhase(); return; }
            //Add cards we selected to keep to player's hand
            foreach (Card c in allSections[4])
                if (c.selected)
                    allSections[6].AddCard(new Card(c));
            //line above ^^ should be player.addCardtoHand
            allSections[6].resetSection(); //reset hand
            allSections[4].isSelectable = false;
            appendErrorMessage("Success!", true);
            //go to next phase
            nextPhase();
        }

        public void setupDevelopPhase(Player player)
        {
            //TODO: Give player discount for choosing this phase
            if (!phasesSelected[1]) { phaseMessage = "Develop phase not chosen. Moving to next phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
            phaseMessage = developPhaseMessage;//"Develop phase: Choose one building from your hand. Then select cards to pay for it.";
            //Clear out phaseSection until we've chosen a thing to build
            allSections[4] = new Section(4, 0, this);
            allSections[4].resetSection();
            //drawCards if have special power
            for (int i = 0; i < player.developDraw; i++)
            {
                player.addCardToHand(deck.Dequeue());
            }
            //player can choose one card from hand to develop
            allSections[6].isSelectable = true;
            allSections[6].maxSelectable = 1;
            allSections[6].minSelectable = 0;
            //Give player temp cost reduction
            if(player.phaseSelected.Equals("II")) player.developReduce++;
        }

        public void developPhase(Card card, int index, int sectionNumber)
        {
            string message = "";
            //We're selecting or deselecting a candidate cards to pay for our development
            if (sectionNumber == 4)
            {
                card.invertSelect();
                if (allSections[6].totalSelected > 0 && (allSections[4].minSelectable > allSections[4].totalSelected))
                    allSections[7].isSelectable = false;
                else
                    allSections[7].isSelectable = true;
                return;
            }
            //We're selecting or deselecting a candidate cards to build
            else if (sectionNumber == 6)
            {
                //we deselected a card, so clear out myPhase section and clear error message
                if (card.selected)
                {
                    card.select(false);
                    allSections[4] = new Section(4, 0, this);
                    allSections[4].maxSelectable = 0;
                    allSections[4].minSelectable = 1;
                    allSections[4].resetSection();
                    allSections[7].isSelectable = true;
                    appendErrorMessage("", true);
                }
                //Otherwise we selected a candidate card to develop
                else
                {
                    //Develop phase is only for buldings
                    if (card.world)
                    {
                        message = "You must select a building, NOT a world.";
                        //This should be the only error possible right now so clear any pre-existing message
                        appendErrorMessage(message, true);
                        return;
                    }
                    int totalCost = (card.cost - players.Peek().developReduce);
                    card.select(true);
                    if (totalCost < 1)
                        allSections[7].isSelectable = true;
                    else
                        allSections[7].isSelectable = false;
                    message = "You have selected " + card.name + ". Choose " + (totalCost) + " cards from to pay for it or choose a different card from your hand to develop.";
                    appendErrorMessage(message, true);
                    //copy over hand except for the card we selected
                    allSections[4] = new Section(4, allSections[6], this);
                    allSections[4].Remove(card);
                    allSections[4].resetSection();
                    //setup phase section to handle developing the card that was selected
                    allSections[4].maxSelectable = totalCost;
                    allSections[4].minSelectable = totalCost;
                    allSections[4].isSelectable = true;
                }
                return;
            }
            //Did we hit the confirm/select button
            else if (sectionNumber != 7 || index != 0) return;
            if (skipPhase) { nextPhase(); return; }
            else if (allSections[6].totalSelected < 1)
            {
                //Players opts out of development phase
                message = "No buildings selected. Not developing anything";
                appendErrorMessage(message, true);
                allSections[4].isSelectable = false;
                //TODO: does nextPhase belong here? I think no
                nextPhase();
            }
            //Paying for card we've chosen
            else
            {
                Card tempCard = allSections[6][allSections[6].lastCardSelected];
                message = "You have successfully developed " + tempCard.name + ".";
                appendErrorMessage(message, true);
                //add card to tableau from hand
                tempCard.select(false);
                allSections[5].AddCard(tempCard);
                allSections[6].Remove(tempCard);
                //these are cards we chose from section 4 to pay for development
                foreach (Card c in allSections[4])
                    if (c.selected)
                        allSections[6].Remove(c);
                allSections[4].isSelectable = false;
                allSections[5].resetSection(); //reset tableau
                allSections[6].resetSection(); //reset hand
                //Draw card if we developed and have special power
                for (int i = 0; i < players.Peek().developDrawAfter; i++)
                {
                    players.Peek().addCardToHand(deck.Dequeue());
                }
                if (players.Peek().developDrawAfter > 0)
                    message += "You drew " + players.Peek().developDrawAfter + "cards.";
                //Remove player temp cost reduction
                if (players.Peek().phaseSelected.Equals("II")) players.Peek().developReduce--;
                //go to next phase
                nextPhase();
            }
        }

        public void setupSettlePhase(Player player)
        {
            //TODO: Give player power to draw card for choosing this phase
            if (!phasesSelected[2]) { phaseMessage = "Settle phase not chosen. Moving to next phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
            phaseMessage = settlePhaseMessage;//"Settle phase: Choose one world from your hand. Then select cards to pay for it.";
            //Clear out phaseSection until we've chosen a thing to build
            allSections[4] = new Section(4, 0, this);
            allSections[4].resetSection();
            //player can choose one card from hand to develop
            player.hand.isSelectable = true;
            player.hand.maxSelectable = 1;
            player.hand.minSelectable = 0;
            //Give player temp card draw
            if (player.phaseSelected.Equals("III")) player.settleDrawAfter++;

        }

        public void settlePhase(Card card, int index, int sectionNumber)
        {
            string message = "";
            //We're selecting or deselecting a candidate cards to pay for our settlement
            if (sectionNumber == 4)
            {
                card.invertSelect();
                //allow user to pick from section 5 if nothing in section 4
                if (allSections[4].totalSelected < 1)
                {
                    foreach (Card car in allSections[5])
                        if (car.hasBorder)
                        {
                            allSections[5].isSelectable = true;
                            break;
                        }
                }
                else
                    allSections[5].isSelectable = false;
                //Make sure we've picked enough cards to settle the world, if we picked one
                if (allSections[6].totalSelected > 0 && (allSections[4].minSelectable > allSections[4].totalSelected))
                    allSections[7].isSelectable = false;
                else
                    allSections[7].isSelectable = true;
                return;
            }
            if (sectionNumber == 6)
            {
                //we deselected a card, so clear out myPhase section and reset message, make sure confirm button is selectable
                if (card.selected)
                {
                    card.select(false);
                    allSections[4] = new Section(4, 0, this);
                    allSections[4].maxSelectable = 0;
                    allSections[4].minSelectable = 1;
                    allSections[4].resetSection();
                    //Don't let them touch tableau unless candidate world is selected
                    allSections[5].isSelectable = false;
                    //Unselect anything in section 5 and remove borders
                    foreach (Card car in allSections[5])
                    {
                        car.hasBorder = false;
                        if(car.selected) car.select(false);
                    }
                    appendErrorMessage("", true);
                }
                //Picking a world to settle
                else
                {
                    if (!card.world)
                    {
                        message = "You must select a world, NOT a building.";
                        appendErrorMessage(message, true);
                        return;
                    }
                    card.select(true);
                    //We've selected a card to develop, allow them to choose cards from tableau to use and discard but only if they have discard power
                    if (card.military)
                    {
                        //Total normal military + rebel bonuses + specific color bonuses
                        int totalMilitary = players.Peek().totalMilitary + Convert.ToInt16(card.rebel) * players.Peek().rebelHelp;
                        //WHAT TO DO ABOUT PRODUCE COLOR? Add as separate attribute on card?
                        if (!String.IsNullOrEmpty(card.windfall)) totalMilitary += players.Peek().settleSpecificMilitary[card.produceColor];
                        if (!String.IsNullOrEmpty(card.produce)) totalMilitary += players.Peek().settleSpecificMilitary[card.produceColor];
                        message = "You have selected " + card.name + ". It requires " + card.cost + " military to settle. You have " + totalMilitary + " military. ";
                        //Not enough miltiary
                        if (card.cost > totalMilitary)
                        {
                            allSections[7].isSelectable = false;
                            message += "\nYou do not have enough military to conquer this world.";
                            //If we don't have any cards to discard don't offer the option
                            if (players.Peek().cardsWithTempMilitary < 1) 
                            {
                                //Only offer contact specialists if military is insufficient
                                if (players.Peek().payForMilitary) //do weird things
                                {
                                    int cost = card.cost - players.Peek().settleReduce;
                                    message += "\nYou can pay for this military world using cards from your hand. Choose " + cost + " cards from to pay for it.";
                                    if (cost < 1)
                                        allSections[7].isSelectable = true;
                                    //copy over hand except for the card we selected
                                    allSections[4] = new Section(4, allSections[6], this);
                                    allSections[4].Remove(card);
                                    allSections[4].resetSection();
                                    //setup phase section to handle developing the card that was selected
                                    allSections[4].maxSelectable = cost;
                                    allSections[4].minSelectable = cost;
                                    allSections[4].isSelectable = true;
                                    foreach (Card car in allSections[5])
                                    {
                                        //IF HAS FREE WORLD POWER IN TABLEAU, highlight it
                                        if (car.freeWorld)
                                        {
                                            car.hasBorder = true;
                                            allSections[5].maxSelectable++;
                                            break;
                                        }
                                    }

                                }
                                appendErrorMessage(message, true); return;
                            }
                            //Otherwise setup discarding cards from tableau
                            message += "You can discard a card from your tableau to gain more military.";
                            //copy over any cards that can be discarded for military
                            //allSections[4] = new Section(4, allSections[5].Count, this);
                            foreach (Card car in allSections[5])
                            {
                                //IF HAS TEMP MILITARY IN TABLEAU, highlight it
                                if (car.tempMilitary > 0)
                                {
                                    car.hasBorder = true;
                                    //Allow user to select multiple military discards
                                    allSections[5].maxSelectable++;
                                }
                            }
                            allSections[5].minSelectable = 0;
                            allSections[5].isSelectable = true;
                            allSections[5].resetSection();
                            //Only offer contact specialists if military is insufficient
                            if (players.Peek().payForMilitary) //do weird things
                            {
                                int cost = card.cost - players.Peek().settleReduce;
                                message += "You can pay for this military world using cards from your hand. Choose " + cost + " cards from to pay for it.";
                                if (cost < 1)
                                    allSections[7].isSelectable = true;
                                //copy over hand except for the card we selected
                                allSections[4] = new Section(4, allSections[6], this);
                                allSections[4].Remove(card);
                                allSections[4].resetSection();
                                //setup phase section to handle developing the card that was selected
                                allSections[4].maxSelectable = cost;
                                allSections[4].minSelectable = cost;
                                allSections[4].isSelectable = true;
                                foreach (Card car in allSections[5])
                                {
                                    //IF HAS FREE WORLD POWER IN TABLEAU, highlight it
                                    if (car.freeWorld)
                                    {
                                        car.hasBorder = true;
                                        allSections[5].maxSelectable++;
                                        break;
                                    }
                                }

                            }

                        }
                        //We have enough military to conquer this
                        else
                        {
                            message += "\nTo conquer this world press continue.";
                            allSections[7].isSelectable = true;
                        }
                    }
                    //this is a regular world we must pay for with cards
                    else
                    {
                        //cost of planet less normal reductions and color specific reductions
                        int cost = card.cost - players.Peek().settleReduce;
                        if (cost < 1)
                            allSections[7].isSelectable = true;
                        else
                            allSections[7].isSelectable = false;
                        if (!String.IsNullOrEmpty(card.windfall)) cost -= players.Peek().settleSpecificCost[card.produceColor];
                        if (!String.IsNullOrEmpty(card.produce)) cost -= players.Peek().settleSpecificCost[card.produceColor];
                        message = "You have selected " + card.name + ". Choose " + cost + " cards from to pay for it or choose a different card from your hand to settle.";
                        //copy over hand except for the card we selected
                        allSections[4] = new Section(4, allSections[6], this);
                        allSections[4].Remove(card);
                        allSections[4].resetSection();
                        //setup phase section to handle developing the card that was selected
                        //always need to be able to select at least one -- WHY??
                        allSections[4].maxSelectable = cost;
                        allSections[4].minSelectable = cost;
                        allSections[4].isSelectable = true;

                        foreach (Card car in allSections[5])
                        {
                            //IF HAS FREE WORLD POWER IN TABLEAU, highlight it
                            if (car.freeWorld)
                            {
                                car.hasBorder = true;
                                allSections[5].minSelectable = 0;
                                allSections[5].maxSelectable = 1;
                                allSections[5].isSelectable = true;
                                break;
                            }
                        }
                    }
                    appendErrorMessage(message, true);
                }
                return;
            }
            //Check if selected card from tableau
            else if (sectionNumber == 5)
            {
                Card tempCard = allSections[6][allSections[6].lastCardSelected];
                //we deselected a card, so clear out myPhase section and reset message, make sure confirm button is selectable
                if (card.selected)
                {
                    card.select(false);
                    players.Peek().tempMilitary -= card.tempMilitary;
                    //allow user to pick from section 4 if nothing in section 5
                    if(allSections[5].totalSelected < 1)
                        allSections[4].isSelectable = true;
                    int totalMilitary = players.Peek().totalMilitary + Convert.ToInt16(tempCard.rebel) * players.Peek().rebelHelp + players.Peek().tempMilitary;
                    if (tempCard.cost > totalMilitary && tempCard.military)
                        allSections[7].isSelectable = false;
                    else if (card.freeWorld && !tempCard.military)
                        allSections[7].isSelectable = false;
                    //TODO:Need to update message to remove most recent line added in next else/if statements below
                }
                //Not a discardable card so ignore and throw warning
                else if (!card.hasBorder)
                {
                    message = "This card cannot be used. Please select one of the highlighted cards.";
                    appendErrorMessage(message, false);
                }
                //Assuming that card has either tempMilitary OR Free world ability, check for military ability if conquering military world
                else if (card.tempMilitary > 0)
                {
                    card.select(true);
                    //Don't allow them to select sections 4 and 5
                    allSections[4].isSelectable = false;
                    //TODO: Need to add all selected cards to total military. Not just most recently clicked
                    players.Peek().tempMilitary += card.tempMilitary;
                    int totalMilitary = players.Peek().totalMilitary + Convert.ToInt16(tempCard.rebel) * players.Peek().rebelHelp + players.Peek().tempMilitary;
                    //Display delta and total military for discarding this, wait for confirm
                    message += "You can gain " + players.Peek().tempMilitary + " military for one turn by discarding these cards.";
                    //TODO: properly update this error message
                    message += "Then  you will have " + totalMilitary + ". You require " + tempCard.cost + " military to settle " + tempCard.name + ".";
                    message += "Press continue to discard.";
                    //Not allowed to discard if still insufficient military.
                    if (totalMilitary < tempCard.cost)
                    {
                        message += "This is not enough. Select more cards to discard or choose a different world to settle/conquer.";
                    }
                    else
                    {
                        message += "Press continue to discard.";
                        allSections[7].isSelectable = true;
                    }
                    appendErrorMessage(message, false);
                }
                //OR check for free world ability if settling a normal world
                else if (card.freeWorld)
                {
                    card.select(true);
                    //Don't allow them to select sections 4 and 5
                    allSections[4].isSelectable = false;
                    //Wait for confirm
                    message = "You have selected " + card.name + ". It can be discarded to settle any world at 0 cost. ";
                    message += "\nTo discard this card and settle the world, press continue.";
                    allSections[7].isSelectable = true;
                    appendErrorMessage(message, false);

                }
                return;
            }
            else if (sectionNumber != 7 || index != 0) return;
            //TODO: Make it so that continue button is not available unless enough things have been selected or military is high enough
            //button is confirm/select button
            if (skipPhase) { nextPhase(); return; }
            else if (allSections[6].totalSelected < 1)
            {
                //Players opts out of settling phase
                message = "No worlds selected. Not settling anything";
                appendErrorMessage(message, true);
                nextPhase();
                return;
            }
            else
            {
                Card tempCard = allSections[6][allSections[6].lastCardSelected];
                message += "You have successfully settled/conquered " + tempCard.name + ".";
                appendErrorMessage(message, true);
                tempCard.select(false);
                //add card to tableau from hand
                allSections[6].Remove(tempCard);
                allSections[5].AddCard(tempCard);
                foreach (Card c in allSections[4])
                    if (c.selected)
                        allSections[6].Remove(c);
                allSections[5].isSelectable = false;
                allSections[5].resetSection(); //reset tableau
                allSections[6].isSelectable = false;
                allSections[6].resetSection(); //reset hand
                //Draw card if we settled and have special power
                for (int i = 0; i < players.Peek().settleDrawAfter; i++)
                {
                    players.Peek().addCardToHand(deck.Dequeue());
                }
                if (players.Peek().settleDrawAfter > 0)
                    message += "You drew " + players.Peek().settleDrawAfter + "cards after settling.";
                //Remove player temp cost reduction
                if (players.Peek().phaseSelected.Equals("III")) players.Peek().settleDrawAfter--;
                nextPhase();
            }
        }

        public void setupTradePhase(Player player)
        {
            //TODO: Don't show anything if player didn't choose trade power
            //If we didn't pick trade, move to regular consume
            if (!player.phaseSelected.Equals("$")) { phaseMessage = "Didn't pick Trade. Moving to Consume phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
            allSections[4] = new Section(4, 2, this);
            //Move all cards from tableau, with goods, to section 4
            foreach (Card c in allSections[5])
            {
                if (c.hasGood)
                {
                    allSections[4].AddCard(new Card(c));
                }
            }
            if (allSections[4].Count < 1)
            {
                phaseMessage = "You chose trade phase but have no goods. You can't do anything. Doofus";
                allSections[7].isSelectable = true;
                //nextPhase();
                return;
            }
            phaseMessage = "Choose one good to trade in for cards.";
            allSections[4].minSelectable = 1;
            allSections[4].maxSelectable = 1;
            allSections[4].isSelectable = true;
            allSections[4].resetSection();
            return;


        }

        public void tradePhase(Card card, int index, int sectionNumber)
        {
            string message = "";
            //We're selecting or deselecting a candidate card to trade
            if (sectionNumber == 4)
            {
                card.invertSelect();
                //Make sure we've picked enough cards to trade (always 1?)
                if (allSections[4].minSelectable > allSections[4].totalSelected)
                    allSections[7].isSelectable = false;
                else
                    allSections[7].isSelectable = true;
                return;
            }
            else if (sectionNumber != 7 && index != 0) { return; }
            //check color of good and draw that many cards for player
            int numCards = 2;
            numCards += players.Peek().tradeDraw + card.tradeThis;
            if (card.produceColor != null) numCards += players.Peek().tradeSpecific[card.produceColor];
            for (int i = 0; i < numCards; i++)
            {
                allSections[6].AddCard(deck.Dequeue());
            }
            allSections[6].resetSection();
            card.hasGood = false;
            message = "You traded for " + numCards + " cards";
            Game1.appendErrorMessage(message,true);
            nextPhase();
        }

        public void setupConsumePhase(Player player)
        {
            if (!phasesSelected[3]) { phaseMessage = "Consume phase not chosen. Moving to next phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
            bool canConsume = false;
            foreach(Card c in allSections[5])
            {
                if (c.consumeAll || c.consumePower != null)
                {
                    canConsume = true;
                    c.hasBorder = true;
                }
            }
            if (canConsume)
            {
                canConsume = !playerConsumeComplete(player);
            }
            if (!canConsume) { phaseMessage = "No available consume powers. Mobing to bext phae"; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
            //Set up regular consume phase
            phaseMessage = "Choose a card with a consume power you wish to use.";
            allSections[5].minSelectable = 1;
            allSections[5].maxSelectable = 1;
            allSections[5].isSelectable = true;
            allSections[7].AddCard(deck.Dequeue());
            allSections[7].isSelectable = true;
            allSections[7].resetSection();
            
        }

        public void consumePhase(Card card, int index, int sectionNumber)
        {
            string message = "";
            //We're selecting or deselecting a candidate cards to pay for our consume power
            if (sectionNumber == 4)
            {
                card.invertSelect();
                //Make sure we've picked enough cards to satisfy the consume power
                //Probably should be it's own tag
                allSections[7][1].hasBorder = canCardBeConsumed(allSections[5][allSections[5].lastCardSelected]);
                return;
            }
            //Selecting or deselecting card with consume power
            else if (sectionNumber == 5)
            {
                //Deselecting a card
                if (card.selected)
                {
                    card.select(false);
                    allSections[4] = new Section(4, 0, this);
                    allSections[4].isSelectable = false;
                    allSections[4].resetSection();
                }
                //Selecting a card
                else
                {
                    //Check for consume power
                    if (!card.hasBorder)
                    {
                        message = "This card doesn't have an available consume power. Pick a different one.";
                        appendErrorMessage(message, true);
                        return;
                    }
                    card.select(true);
                    //copy over everything with a good
                    allSections[4] = new Section(4, allSections[5].Count, this);
                    //TODO: seutp other consume effects, not all cards will have consumPower!=null
                    if(card.consumePower != null) validateConsumePower(card);
                    else if (card.consumeAll)
                    {
                        //TODO: Notify user how they're about to lose all cards and gain X-1 VPs
                    }
                    allSections[4].resetSection();
                }
            }
            //TODO: Decide if having a separate advance button would be helpful, I think it would
            if (sectionNumber != 7) return;
            //approximating card isSelectable using hasBorder
            if (!card.hasBorder) return;
            if (index == 1)
            {
                Card tempCard = allSections[5][allSections[5].lastCardSelected];
                ConsumePowers tempPower = tempCard.consumePower;
                tempPower.usesLeft--;
                Player p = players.Peek();
                if (tempPower.usesLeft < 1) { tempCard.hasBorder = false; p.myConsumePowers.Remove(tempPower); }
                p.chipPoints += tempPower.vpReward;
                for (int i = 0; i < tempPower.cardReward; i++)
                {
                    allSections[6].AddCard(deck.Dequeue());
                }
                allSections[6].resetSection();
                allSections[7][0].isSelectable = playerConsumeComplete(p);
            }
            allSections[7].RemoveAt(1);
            //If successful remove borders
            foreach (Card c in allSections[5])
            {
                c.hasBorder = false;
            }
        }

        public bool playerConsumeComplete(Player player)
        {
            if (player.myConsumePowers.Count > 0) return true;
            //TODO check if 'all' has been used - aka check if any goods are left
            return false;
        }

        public bool canCardBeConsumed(Card card)
        {
            //Make sure we've chosen enough goods/cards to consume
            if (allSections[4].minSelectable > allSections[4].totalSelected)
                return false;
            //Assume all typeOfGood's have been validated except different
            if (card.consumePower.typeOfGood.Equals("different"))
            {
                string typesOfGoods = ",";
                bool isDifferent = true;
                foreach (Card c in allSections[4])
                {
                    if (!c.selected) break;
                    if (typesOfGoods.Contains(c.produceColor)) {isDifferent = false; break;}
                    typesOfGoods += c.produceColor + ",";
                }
                if (!isDifferent) return false;
            }
            return true;
        }

        public void validateConsumePower(Card card)
        {
            ConsumePowers tempPower = card.consumePower;
            switch (tempPower.typeOfGood)
            {
                case ("card"):
                    foreach (Card c in allSections[6])
                    {
                        allSections[4].AddCard(new Card(c));
                    }
                    break;
                case ("any"):
                    foreach (Card c in allSections[5])
                    {
                        if (c.hasGood)
                            allSections[4].AddCard(new Card(c));
                    }
                    break;
                case ("different"):
                    foreach (Card c in allSections[5])
                    {
                        if (c.hasGood)
                            allSections[4].AddCard(new Card(c));
                    }
                    break;
                default:
                    foreach (Card c in allSections[5])
                    {
                        if (c.hasGood && c.produceColor.Equals(tempPower.typeOfGood))
                            allSections[4].AddCard(new Card(c));
                    }
                    break;
            }
            //only allow people to do these one at a time, even if power can be used multiple times
            allSections[4].minSelectable = tempPower.numOfGoods;
            allSections[4].maxSelectable = tempPower.numOfGoods;
        }

        public void setupProducePhase(Player player)
        {
            if (!phasesSelected[4]) { phaseMessage = "Produce phase not chosen. Moving to next phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
        }
        public void SetupUpKeepPhase(Player player)
        {
            if (!phasesSelected[4]) { phaseMessage = "Upkeep phase not chosen. Moving to next phase."; allSections[4] = new Section(4, 0, this); allSections[4].minSelectable = 0; skipPhase = true; return; }
        }

        public static void appendErrorMessage(String message, bool clearMessage)
            //clear message flag will wipe the error message
        {
            if (clearMessage)
            {
                errorMessage = "";
            }
            if (String.IsNullOrEmpty(errorMessage)) errorMessage = message;
            else if (errorMessage.Contains(message)) return;
            else errorMessage += ("\n" + message);
        }

        protected void onWindowResize()
        {
            // Make changes to handle the new window size.
            MaxY = graphics.GraphicsDevice.Viewport.Height;
            MaxX = graphics.GraphicsDevice.Viewport.Width;
            x0 = MaxX / 2F;
            y0 = MaxY / 4F; y1 = MaxY / 2; y2 = (int) y0 * 3;
            cardScale = ((MaxY / 4) / bufferSum) / textureHeight;
            cardHeight = (int)(textureHeight * cardScale);
            cardWidth = (int)(textureWidth * cardScale);

            initSections();
        }

        public void cardClicked(Card c, int cardIndex, int sectionNumber)
        {
            switch (currentPhase)
            {
                case "Selection":
                    this.selectPhase(c, cardIndex, sectionNumber);
                    break;
                case "Explore":
                    this.explorePhase(c, cardIndex, sectionNumber);
                    break;
                case "Develop":
                    this.developPhase(c, cardIndex, sectionNumber);
                    break;
                case "Settle":
                    this.settlePhase(c, cardIndex, sectionNumber);
                    break;
                case "Trade":
                    this.tradePhase(c, cardIndex, sectionNumber);
                    break;
                case "Consume":
                    this.consumePhase(c, cardIndex, sectionNumber);
                    break;
                case "Wait":
                    nextPhase();
                    break;
                default:
                    break;
            }
        }
    }
}
