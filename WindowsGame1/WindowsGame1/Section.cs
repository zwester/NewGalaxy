using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    public class Section : List<Card>
    {
        public int xShift, yShift;
        protected int isOneMouseOver;
        public int maxSize, maxSelectable, minSelectable, sectionNumber, lastCardSelected, totalSelected;
        public float handOverlap;
        //is this section active for selection
        public bool isSelectable;
        public Game1 myGame;

        public Section(int numCards, Game1 game) : base(numCards)
        {
            lastCardSelected = -1;
            isSelectable = false;
            isOneMouseOver = -1;
            totalSelected = 0;
            minSelectable = maxSelectable = 1;
            yShift = 1;
            xShift = 0;
            sectionNumber = -1;
            myGame = game;
            calcHandOverlap();
        }

        public Section(int section,int numCards, Game1 game) : base(numCards)
        {
            lastCardSelected = -1;
            isSelectable = false;
            isOneMouseOver = -1;
            totalSelected = 0;
            minSelectable = maxSelectable = 1;
            sectionNumber = section;
            myGame = game;
            calcXYShift(section);
            calcHandOverlap();
        }

        public Section(int section, Section sect, Game1 game) : base(sect.Capacity)
        {
            for (int i = 0; i < sect.Count; i++)
            {
                this.AddCard(new Card(sect[i]));
            }
            lastCardSelected = -1;
            isSelectable = false;
            isOneMouseOver = -1;
            totalSelected = 0;
            minSelectable = maxSelectable = 1;
            sectionNumber = section;
            myGame = game;
            calcXYShift(section);
            calcHandOverlap();
        }

        public void AddCard(Card item)
        {
            //base add
            item.selected = false;
            item.z= item.x = item.y = 0;
            item.size = new Rectangle(item.x, item.y, 0, 0);
            item.mouseOver = false;
            item.curSection = this;
            this.Add(item);
            calcHandOverlap();
        }

        public void calcXYShift(int section)
        {
            switch (section)
            {
                case 0:
                    yShift = (int)Game1.y0;
                    xShift = 0;
                    //hand = tableauOne;
                    break;
                case 1:
                    yShift = (int) Game1.y0;
                    xShift = (int) Game1.x0;
                    //hand = tableauTwo;
                    break;
                case 2:
                    yShift = Game1.y1;
                    xShift = 0;
                    //hand = tableauThree;
                    break;
                case 3:
                    yShift = Game1.y1;
                    xShift = (int) Game1.x0;
                    //hand = allActions;
                    break;
                case 4:
                    yShift = Game1.y2;
                    xShift = 0;
                    //hand = myPhase;
                    break;
                case 5:
                    yShift = Game1.y2;
                    xShift = (int) Game1.x0;
                    //hand = players.Peek().tableau;
                    break;
                case 6:
                    yShift = Game1.MaxY;
                    xShift = 0;
                    //hand = players.Peek().hand;
                    break;
                case 7:
                    yShift = Game1.MaxY;
                    xShift = (int) Game1.x0;
                    //hand = myActions;
                    break;
                default:
                    return;
            }
        }

        public void calcHandOverlap()
        {
            //Cards must fit on half of screen width
            //handOverlap is actually how much of the card is not overlapped
            handOverlap = (Game1.MaxX / 2.0F - 4 * Game1.xBuffer) / (this.Count * Game1.cardWidth) * 1.0F;
            if (handOverlap > 1) handOverlap = 1;
        }

        public void updateSection(MouseState ms, bool hasBeenReleased)
        {
            int index;

            //Looping over cards from right to left
            foreach (Card c in this.Reverse<Card>())
            {
                //has mouse button been released snce it was clicked?
                if (!hasBeenReleased) continue;
                index = this.FindIndex(xs => xs.Equals(c));
                //if earlier card has been mousedOver, this one can't be
                if (isOneMouseOver != index && isOneMouseOver >= 0)
                {
                    //if previously moused over card is still moused over then this one can't be
                    Card car = new Card(this);
                    car = this[isOneMouseOver];
                    if (car.size.Contains(ms.X, ms.Y)) c.mouseOver = false;
                    //if the previous one isn't moused over anymore, this card could be moused over
                    else c.mouseOver = c.size.Contains(ms.X, ms.Y);
                }
                //otherwise, mouseover for that card is if mouse position is within bounds
                else c.mouseOver = c.size.Contains(ms.X, ms.Y);
                //we clicked a card
                if (hasBeenReleased && c.mouseOver && ms.LeftButton == ButtonState.Pressed && (c.selected || totalSelected < maxSelectable) && isSelectable)
                {
                    //Game1.sectionClicked = sectionNumber;
                    //Move all this to cardClicked function? or to Game1 class?
                    {
                        //invert selected state
                        //c.selected = !c.selected;
                        //lastCardSelected = lastCardSelected + (Convert.ToInt16(c.selected) * 2 - 1) * (index + 1);
                        //Need to do this here so we don't select a card for one frame that isn't allowed to be selected
                        //cardClicked(c, index);
                        //update total selected count
                        //totalSelected = totalSelected + (2 * Convert.ToInt16(c.selected) - 1);//if selected add 1 total,if deselect subtract 1 from total
                    }
                    myGame.cardClicked(c, index, sectionNumber);
                    //update mouse release state
                    hasBeenReleased = false;
                }
                //we moused over this one
                if (c.mouseOver)
                {
                    //update which card we are moused over
                    isOneMouseOver = index;
                    //update position and size to reflect mousedOver card size
                    c.x = (int)(xShift + Game1.cardWidth * index * handOverlap - Game1.mouseOverOffset * Game1.cardWidth + Game1.xBuffer);
                    c.y = (int)(yShift - (Game1.cardHeight + Game1.yBuffer) - Game1.mouseOverOffset * Game1.cardHeight - Game1.selectOffsetY * Game1.cardHeight * Convert.ToInt16(c.selected));
                    c.z = 1;
                    c.size = new Rectangle(c.x, c.y, (int)(Game1.cardWidth * Game1.mouseOverScale), (int)(Game1.cardHeight * Game1.mouseOverScale));
                }
                //not moused over this one
                else
                {
                    //we used to be mouse over this one but no longer
                    if (isOneMouseOver == index) isOneMouseOver = -1;
                    //moused over card is to the left of this one, shift right
                    if (isOneMouseOver > -1 && isOneMouseOver < index)
                    {
                        c.x = (int)(xShift + Game1.xBuffer + Game1.cardWidth * index * handOverlap + Game1.mouseOverOffset * Game1.cardWidth + (1 - handOverlap) / 2F * Game1.cardWidth);
                        c.z = 1 - (1F / this.Count * index) + .01F;
                    }
                    //moused over card is to the right of this one, shift left
                    else if (isOneMouseOver > index)
                    {
                        c.x = (int)(xShift + Game1.xBuffer + Game1.cardWidth * index * handOverlap - Game1.mouseOverOffset * Game1.cardWidth - (1 - handOverlap) / 2F * Game1.cardWidth);
                        c.z = 1F / this.Count * index + 0.1F;
                    }
                    //nothing is moused over, reset to normal
                    else
                    {
                        c.x = (int)(xShift + Game1.xBuffer + Game1.cardWidth * index * handOverlap);
                        c.z = 1F/this.Count * index + .01F;
                    }
                    //no idea
                    c.y = (int)(yShift - (Game1.cardHeight + Game1.yBuffer) - Game1.selectOffsetY * Game1.cardHeight * Convert.ToInt16(c.selected));
                    c.size = new Rectangle(c.x, c.y, (int)(Game1.cardWidth), (int)(Game1.cardHeight));
                }
            }
        }

        public void resetSection()
        {
            int index;

            foreach (Card c in this.Reverse<Card>())
            {
                isOneMouseOver = -1;
                index = this.FindIndex(xs => xs.Equals(c));
                //nothing is moused over, reset to normal
                c.x = (int)(xShift + Game1.xBuffer + Game1.cardWidth * index * handOverlap);
                //no idea
                c.y = (int)(yShift - (Game1.cardHeight + Game1.yBuffer) - Game1.selectOffsetY * Game1.cardHeight * Convert.ToInt16(c.selected));
                c.z = index * 1F / this.Count + .01F;
                c.size = new Rectangle(c.x, c.y, (int)(Game1.cardWidth), (int)(Game1.cardHeight));
            }
        }

        public void cardClicked(Card c, int index)
        {
            string message = "";
            switch (Game1.currentPhase)
            {
                case "Selection":
                    if (sectionNumber == 7 && lastCardSelected==0)
                    {
                        buttonHandling(c);
                        return;
                    } 
                    break;
                case "Explore":
                    if (sectionNumber == 7 && lastCardSelected == 0)
                    {
                        buttonHandling(c);
                        return;
                    } 
                    break;
                case "Develop":
                    //Choosing which card to build
                    if (sectionNumber == 6)
                    {
                        //we deselected a card, so clear out myPhase section and reset message
                        if (!c.selected)
                        {
                            Game1.allSections[4] = new Section(4, 0, myGame);
                            Game1.allSections[4].maxSelectable = 0;
                            Game1.allSections[4].minSelectable = 1;
                            Game1.allSections[4].resetSection();
                            Game1.appendErrorMessage("", true);
                        }
                        //Choosing the card we want to develop
                        else
                        {
                            if (c.world)
                            {
                                message = "You must select a building, NOT a world.";
                                Game1.appendErrorMessage(message,true);
                                //c.selected = false;
                                //need to offset change in totalSelected that's coming later
                                totalSelected++;
                                lastCardSelected = -1;
                                break;
                            }
                            message = "You have selected "+c.name+". Choose "+(c.cost-Game1.players.Peek().developReduce)+" cards from to pay for it or choose a different card from your hand to develop.";
                            Game1.appendErrorMessage(message,true);
                            //copy over hand except for the card we selected
                            Game1.allSections[4] = new Section(4, Game1.allSections[6], myGame);
                            Game1.allSections[4].Remove(c);
                            Game1.allSections[4].resetSection();
                            //setup phase section to handle developing the card that was selected
                            Game1.allSections[4].maxSelectable = c.cost - Game1.players.Peek().developReduce;
                            Game1.allSections[4].minSelectable = c.cost - Game1.players.Peek().developReduce;
                            Game1.allSections[4].isSelectable = true;
                        }
                    }
                    //Hitting confirm button
                    else if (sectionNumber == 7 && lastCardSelected == 0)
                    {
                        //button is confirm/select button
                        c.selected = false;
                        lastCardSelected = -1;
                        if (Game1.skipPhase) { Game1.confirmClick = true; break; }
                        else if (Game1.allSections[6].totalSelected < 1)
                        {
                            //Players opts out of development phase
                            message = "No buildings selected. Not developing anything";
                            Game1.appendErrorMessage(message, true);
                            Game1.confirmClick = true;
                        }
                        //Paying for card we've chosen
                        else
                        {
                            Card tempCard = Game1.allSections[6][Game1.allSections[6].lastCardSelected];
                            //Didn't choose enough cards
                            if ((Game1.allSections[4].minSelectable) > (Game1.allSections[4].totalSelected))
                            {
                                message = "You must select at least " + tempCard.cost + " cards.";
                                Game1.appendErrorMessage(message, false);
                            }
                            else
                            {
                                message = "You have successfully developed " + tempCard.name+".";
                                Game1.appendErrorMessage(message, true);
                                Game1.confirmClick = true;

                                //Draw card if we developed and have special power
                                for (int i = 0; i < Game1.players.Peek().developDrawAfter; i++)
                                {
                                    Game1.players.Peek().addCardToHand(Game1.deck.Dequeue());
                                }
                            }
                        }
                    }
                    break;
                case "Settle": //should be similar to build, except military...:(
                    //NEED TO ADD IF CLAUSE FOR SECTION 5 (tableau)???
                    //Choosing which card to settle
                    if (sectionNumber == 6)
                    {
                        //we deselected a card, so clear out myPhase section and reset message, make sure confirm button is selectable
                        if (!c.selected)
                        {
                            Game1.allSections[4] = new Section(4, 0, myGame);
                            Game1.allSections[4].maxSelectable = 0;
                            Game1.allSections[4].minSelectable = 1;
                            Game1.allSections[4].resetSection();
                            Game1.allSections[7].isSelectable = true;
                            Game1.allSections[5].isSelectable = false;
                            //TODO:Need to unselect anything in section 5
                            Game1.appendErrorMessage("", true);
                        }
                        //Paying for card we've chosen
                        else
                        {
                            if (!c.world)
                            {
                                message = "You must select a world, NOT a building.";
                                Game1.appendErrorMessage(message, true);
                                c.selected = false;
                                //need to offset change in totalSelected that's coming later
                                totalSelected++;
                                lastCardSelected = -1;
                                break;
                            }

                            //We've selected a card to develop, allow them to choose cards from tableau to use and discard
                            Game1.allSections[5].isSelectable = true;
                            Game1.allSections[5].minSelectable = 0;
                            if (c.military)
                            {
                                //if(!Game1.players.Peek().payForMilitary) //do normal things
                                //else //do weird things
                                //Total normal military + rebel bonuses + specific color bonuses
                                int totalMilitary = Game1.players.Peek().totalMilitary + Convert.ToInt16(c.rebel) * Game1.players.Peek().rebelHelp;
                                //WHAT TO DO ABOUT PRODUCE COLOR? Add as separate attribute on card?
                                if (c.windfall != "") totalMilitary += Game1.players.Peek().settleSpecificMilitary[c.windfall];
                                if (c.produce != "") totalMilitary += Game1.players.Peek().settleSpecificMilitary[c.produce];
                                message = "You have selected " + c.name + ". It requires " + c.cost + " military to settle. You have " + totalMilitary + " military. ";
                                //Not enough miltiary
                                if (c.cost > totalMilitary)
                                {
                                    message += "\nYou do not have enough military to conquer this world.";
                                    //If we don't have any cards to discard don't offer the option
                                    if (Game1.players.Peek().cardsWithTempMilitary < 1) { Game1.appendErrorMessage(message, true); break; }
                                    //Otherwise setup discarding cards from tableau
                                    message += "You can discard a card from your tableau to gain more military.";
                                    Game1.allSections[7].isSelectable = false;
                                    //copy over any cards that can be discarded for military
                                    Game1.allSections[4] = new Section(4, Game1.allSections[5].Count, myGame);
                                    foreach(Card car in Game1.allSections[5])
                                    {
                                    //IF HAS TEMP MILITARY IN TABLEAU, highlight it
                                        if (car.tempMilitary > 0) {
                                            car.hasBorder = true;
                                            //Allow user to select multiple military discards
                                            Game1.allSections[5].maxSelectable++;
                                        }
                                    }
                                    Game1.allSections[4].resetSection();
                                    //setup phase section to handle developing the card that was selected
                                    Game1.allSections[4].maxSelectable = Game1.allSections[4].Count;
                                    Game1.allSections[4].minSelectable = 0;
                                    Game1.allSections[4].isSelectable = true;

                                }
                                else
                                    message += "\nTo conquer this world press continue.";
                                Game1.allSections[4].minSelectable = 0;
                            }
                            else
                            {
                                //cost of planet less normal reductions and color specific reductions
                                int cost = c.cost - Game1.players.Peek().settleReduce;
                                //if (c.windfall != "") cost = cost - Game1.players.Peek().settleSpecificCost[c.windfall];
                                //if (c.produce != "") cost = cost - Game1.players.Peek().settleSpecificCost[c.produce];
                                message = "You have selected " + c.name + ". Choose " + cost + " cards from to pay for it or choose a different card from your hand to settle.";

                                //copy over hand except for the card we selected
                                Game1.allSections[4] = new Section(4, Game1.allSections[6], myGame);
                                Game1.allSections[4].Remove(c);
                                foreach(Card car in Game1.allSections[5])
                                {
                                //IF HAS FREE WORLD POWER IN TABLEAU, highlight it
                                    if (car.freeWorld)
                                    {
                                        car.hasBorder = true;
                                        Game1.allSections[5].maxSelectable = 1;
                                    }
                                }
                                Game1.allSections[4].resetSection();
                                //setup phase section to handle developing the card that was selected
                                //always need to be able to select at least one
                                Game1.allSections[4].maxSelectable = cost; if (Game1.allSections[4].maxSelectable == 0) Game1.allSections[4].maxSelectable = 1;
                                Game1.allSections[4].minSelectable = cost;
                                Game1.allSections[4].isSelectable = true;
                            }
                            Game1.appendErrorMessage(message, true);
                        }
                    }
                    //Check if selected card from tableau
                    else if (sectionNumber == 5)
                    {
                        //we deselected a card, so clear out myPhase section and reset message, make sure confirm button is selectable
                        if (!c.selected)
                        {
                            //TODO:Need to update message to remove most recent line
                        }
                        //Not a discardable card so ignore and throw warning
                        else if (!c.hasBorder)
                        {
                            message = "This card cannot be used. Please select one of the highlighted cards.";
                            Game1.appendErrorMessage(message, false);
                        }
                        //Assuming that card has either tempMilitary OR Free world ability, check for military ability if conquering military world
                        else if (Game1.allSections[6][Game1.allSections[6].lastCardSelected].military)
                        {
                            if (c.tempMilitary > 0)
                            {
                                //Display delta and total military for discarding this, wait for confirm
                                message = "You can gain " + c.tempMilitary + " military for one turn by discarding this card.";
                                //TODO: properly update this error message
                                message = "Then  you will have totalMiltary. You require " + c.cost + " military to settle c.name";
                                message += "Press continue to discard.";
                            }
                        }
                        //OR check for free world ability if settling a normal world
                        else if (c.freeWorld)
                        {
                            //Wait for confirm
                            message = "You have selected " + c.name + ". It can be discarded to settle any world at 0 cost. ";
                            message += "\nTo discard this card and settle the world, press continue.";
                            Game1.appendErrorMessage(message, false);
         
                        }
                    }
                    else if (sectionNumber == 7 && lastCardSelected == 0)
                    {
                        //button is confirm/select button
                        c.selected = false;
                        lastCardSelected = -1;
                        if (Game1.skipPhase) {Game1.confirmClick = true; break; }
                        else if (Game1.allSections[6].totalSelected < 1)
                        {
                            //Players opts out of settling phase
                            message = "No worlds selected. Not settling anything";
                            Game1.appendErrorMessage(message, true);
                            Game1.confirmClick = true;
                        }
                        else
                        {
                            //TODO:Need to check for temp military and free world discards before doing other checks
                            //TODO:If selected from section 4 and 5, that's a problem; don't advance
                            Card tempCard = Game1.allSections[6][Game1.allSections[6].lastCardSelected];
                            if (Game1.allSections[5].totalSelected > 0)
                            {
                                //We selected something from the tableau
                                //TODO:Check that section 4 is empty
                                //TODO:Check that we met minimum military cost if military world
                                //TODO:Remove cards we chose to discard, add settled/conquered world to tableau
                                //TODO: Clear is border flag
                            }
                            //Didn't choose enough cards
                            else if ((Game1.allSections[4].minSelectable) > (Game1.allSections[4].totalSelected))
                            {
                                message = "You must select at least " + tempCard.cost + " cards.";
                                Game1.appendErrorMessage(message, false);
                            }
                            else
                            {
                                message = "You have successfully settled " + tempCard.name + ".";
                                Game1.appendErrorMessage(message, true);
                                Game1.confirmClick = true;

                                //Draw card if we settled and have special power
                                for (int i = 0; i < Game1.players.Peek().settleDrawAfter; i++)
                                {
                                    Game1.players.Peek().addCardToHand(Game1.deck.Dequeue());
                                }
                            }
                        }
                    }
                    break;
                case "Consume":
                    if (Game1.skipPhase) { Game1.confirmClick = true; break; }
                    break;
                default:
                    break;
            }
        }

        public void buttonHandling(Card c)
        {
            //button is confirm/select button
            c.selected = false;
            //need to offset change in totalSelected that's coming later
            totalSelected++;
            lastCardSelected = -1;
            if (Game1.skipPhase) { Game1.confirmClick = true; return; }
            //not enough were selected so display error message and quit
            if (Game1.allSections[4].totalSelected < Game1.allSections[4].minSelectable)
            {
                string message = "You must select at least " + Game1.allSections[4].minSelectable + " cards.";
                Game1.appendErrorMessage(message,false);
                return;
            }
            Game1.errorMessage = "";
            Game1.confirmClick = true;
        }
    }
}
