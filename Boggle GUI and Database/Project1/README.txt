12/10/2014

1. Blank game is recorded successfully on GameInformation database and PlayerInformation database.
2. ExecuteNonQuery, ExecuteReader all execute the command as we suspected.
3. Words are recorded properly.
4. close the game prematurely. Does not record, as it shouldn't. 

Made an edit to BoggleGUI, MainWindow.xaml interaction logic. Prevented the user from entering the empty string.






12/5/2014 3:28AM

No more bugs. Commenting done.

UNIT TESTS CAN'T FUNCTION COLLECTIVELY. WE PASS THEM IF RUN ONE BY ONE, WHICH GIVES SOMETHING LIKE 75% CODE COVERAGE.

We ask for some leniency in grading our unit tests -- after all, the GUI works and handles exceptions gracefully.





12/5/2014, 1:26AM

Bugs fixed. No more bugs. Remaining work

	commenting (names, private variables/properties)
	write unit tests.


Remaining bugs:
1. scores not updating correctly (ERROR: )
2. 
	same usernames
	let the timer run to completion
	null reference exception in GameCommandReceived in BoggleServer.cs
	appears that the GUI resets properly
3.
	same usernames
	one client closes before the other
	KeyNotFoundException in endGame in BoggleServer.cs
	it appears that the other client's GUI resets properly

POTENTIAL FIX TO THE ABOVE: the opponent method identifies players by username
Quick fix: in the CommandReceived methods of BoggleServer, I changed
command == null && e == null to command == null && (e == null || e is ObjectDisposedException)

4. If the games run to completion and sit a while, we still get a null reference exception at line 218






12/4/2014: 2:29AM MZ
The reason why the other client does not respond to termination of the this client is that
the line sent is actually "TERMINATETIME  490\r" (i.e. it is convoluted with time.)
This was fixed in the BoggleServer class, so that terminate was sent with TERMINATE + \r\n

Fixed: connect twice error after other client disconnects
fixed: Reset() sets GUI to defaults that creating the gUI does.

ERROR: IF TWO PEOPLE WITH SAME USERNAME CONNECT, AND ONE EXITS, THE RESETTING IS NOT DONE PROPERLY. PRETTY SURE
ALL ELSE WORKS.


12/4/2014: MZ, AH

1) doesn't do the right thing when TERMINATE is sent back //fixed
2) can't enter words //fixed
3) add a quit button //optional
4) add a reset button //optional



12/1/2014: MZ, AH

if username is typed in, update username
if ip address is typed in, update ip address

play button: invoke connect if username and ip address are non-empty. otherwise, pop up a box.

if receive "TIME", alter the time GUI entry
if receive "SCORE", alter the score entries
if receive "START", alter the user entries (near the scores)

if word is typed in, call SendWord, and update the words sent container.



DEBUGGING

11/25/2014: MZ

While debugging, I tried to step into the CommandReceived method. I thought this did not work because
the StringSockets passed in were different. It turns out that one cannot step into the GameCommandReceived.

Actually, one can. Currently, the GameCommandReceived is not executed fast enough for the score to be updated
before the asserts are called. One should look into optimizing this, but otherwise it seems the code is correct.

In addition, one should handle cases where: one of the clients disconnects (try-catches everywhere?).

We will also want to check that the communication protocols are met when playing the game via terminal.










11/20/2014 -- MZ and AH

BogServer should have a BoggleGame


Game

bool gameIsOngoing

Player1, Player2 -- each have a struct tying their username, score, StringSocket
	 and words sent together

	 maybe a class so that they have a method to calculate scores.


	 


Possible commands from the user: (SWITCH and break on these)
	PLAY @ -- method adds @  to the list of users. if the list has one (or more)
		users in it, create a game with the two users. add it to their struct field.
		create a new string socket for that game and add it to the list of games.
	WORD $ -- method that operates on the word $ and sends SCORE (#1) (#2)
	default: IGNORING <their command>


Constantly: 
	send TIME #	-- # is number of seconds remaining in the game
		if # == 0, ignore the sockets and send the scores. Transmit the game summary.
		if they type stuff in, IGNORING

SCORE CALCULATION

	Suppose that during the game the client played a legal words
	 that weren't played by the opponent, the opponent played b 
	 legal words that weren't played by the client, both players 
	 played c legal words in common, the client played d illegal 
	 words, and the opponent played e illegal words. The game 
	 summary command should be "STOP a #1 b #2 c #3 d #4 e #5", 
	 where a, b, c, d, and e are the counts described above and 
	 #1, #2, #3, #4, and #5 are the corresponding space-separated 
	 lists of words.

	 11/29/2014

	 Created the boggler client class which is a windows form application
	 and the boggle client model class which is a class library.

	 We created a model which will store the socket connection as well as the 
	 game state information. We used a class library because it will be a class that
	 will be placed in the GUI. 
	 
	 Designed the GUI!
	 Put the play button in the middle
	 the word coutn to the right
	 the boggle board to the left
	 and the time and score above the boggle board

	 
	 2 and a half hours.

	 12/4/2014 - AH
	 MVC - Model was not modeled right
	 Removed all references of the GUI from model
	 Organized the class by making renaming Model to BoggleGUI
	 Changed Methods in the GUI to invoke actions that change the GUI
	 Added Action objects to the model
	 Each Action took paremters from the server and sends them to the GUI
	 Organized an action for each command sent from the server
	 Deleted all code that was not relevant to the model

	 3 hours - Including time trying to figure out how actions worked and how GUI worked

	  12/4/2014 - AH

	  Added highlight feature to words
	  First used dictionary to store digital board but realized that that was stupid
	  Then used a list to store the boggle board
	  Fixed indexing for the boggle board
	  Does not work for duplicated letters yet will be fixed in patch
	  Made a default username for people who don't enter a username
	  Made a basic class test which starts to test action events

	  2 1/2 hours - Including time of tests
