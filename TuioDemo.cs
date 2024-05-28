using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TUIO;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

public class TuioDemo : Form , TuioListener
{

		
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public int YearsOfWorking { get; set; }
	}

	public List<User> users;
	public int symbolIndex = 0;
	private Label reco;
	private Label state;
	public int flag2=0;
	public Image menu;
	public Image menu2;
	int currentHour = DateTime.Now.Hour;
	private Label timeLabel;
	private readonly IMongoCollection<BsonDocument> _collection;
	private TuioClient client2;
	private Dictionary<long,TuioObject> objectList;
	private Dictionary<long,TuioCursor> cursorList;
	private Dictionary<long,TuioBlob> blobList;
	private Dictionary<long, float> initialOrientations = new Dictionary<long, float>();
	public float tolerancePercentage = 0.05f; // 5% tolerance
	public float someToleranceValue;
	System.Windows.Forms.Timer tt = new System.Windows.Forms.Timer();

    System.Windows.Forms.Timer tttime = new System.Windows.Forms.Timer();

	public static int width, height;
	private int window_width =  640;
	private int window_height = 480;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;

	private bool fullscreen;
	private bool verbose;

	string imagePath = "table.jpeg";
	string apple = "apple.png";
	string lemon = "lemon.png";
	string pineapple = "pineapple.png";
	string plate = "plate.png";
	string blackPlate = "blackPlate.png";

	//breakfast
	string omelette = "omelette.png";
	string toast = "toast.png";
	string pancake = "pancake.png";

	//launch
	string pasta = "pasta.png";
	string steak = "steak.png";
	string pizza = "pizza.png";

	//dinner
	string flafel = "flafel.png";
	string fries = "fries.png";
	string cake = "cake.png";
    private int quitFlag = 0;


    string fork = "fork.png";
	string knife = "knife.png";

	Font font = new Font("Arial", 10.0f);
	SolidBrush fntBrush = new SolidBrush(Color.White);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0,0,64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);


		
	public TuioDemo(int port) {
		someToleranceValue = (float)Math.PI / 2 * tolerancePercentage;
		CreateButton();	
		CreateButton2();	

		var client = new MongoClient("mongodb://localhost:27017");
		var database = client.GetDatabase("phaseone");
		_collection = database.GetCollection<BsonDocument>("test");


		state = new Label();
		//state.Text = "Customer State: Happy";
		state.Location = new Point((this.ClientSize.Width / 2), (this.ClientSize.Height / 2));
		state.Size = new Size(300, 50);
		state.Font = new Font("Arila", 20);
		state.BackColor = Color.RosyBrown;
		this.Controls.Add(state);
		verbose = false;
		fullscreen = false;
		width = window_width;
		height = window_height;

		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";
			
			
		this.Closing+=new CancelEventHandler(Form_Closing);
		this.KeyDown+=new KeyEventHandler(Form_KeyDown);

		this.SetStyle( ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long,TuioObject>(128);
		cursorList = new Dictionary<long,TuioCursor>(128);
		blobList   = new Dictionary<long,TuioBlob>(128);
			
		client2 = new TuioClient(port);
		client2.addTuioListener(this);

		client2.connect();
		InitializeComponent();
	}

    

	private void CreateButton()
	{
		// Create a new button instance
		Button myButton = new Button();

		// Set properties of the button
		myButton.Text = "Click Me";
		myButton.Size = new Size(200, 60); // Width, Height
		myButton.Location = new Point(420, 0); // X, Y

		// Handle the Click event
		myButton.Click += new EventHandler(MyButton_Click);

		// Add the button to the form's controls
		this.Controls.Add(myButton);
	}
	private void CreateButton2()
	{
		// Create a new button instance
		Button myButton2 = new Button();

		// Set properties of the button
		myButton2.Text = "Socket";
		myButton2.Size = new Size(100, 40); // Width, Height
		myButton2.Location = new Point(10, this.window_height - 80); // X, Y

		// Handle the Click event
		myButton2.Click += new EventHandler(MyButton_Click2);

		// Add the button to the form's controls
		this.Controls.Add(myButton2);
	}
    public async void MyButton_Click2(object sender, EventArgs e)
    {
        await Task.Run(() => CallPythonServiceAsync());
    }

    public async void MyButton_Click(object sender, EventArgs e)
	{
    // Clear any existing text
			

		try
		{
			// Retrieve all documents from the collection and deserialize them into User objects
			var filter = Builders<BsonDocument>.Filter.Empty;
			var documents = await _collection.Find(filter).ToListAsync();
			
			// Convert the BsonDocuments to User objects
			users = documents.Select(doc => new User
			{
				Id = doc["_id"].AsInt32,
				Name = doc["name"].AsString,
				Age = doc["age"].AsInt32,
				YearsOfWorking = doc["yearsOfWorking"].AsInt32
			}).ToList();

			// Display or process the user objects
			// For example, to display the first user's name:
				

		}
		catch (Exception ex)
		{
			MessageBox.Show("Error connecting to MongoDB: " + ex.Message);
		}
			
	}
    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

        if (e.KeyCode == Keys.Q)
        {
            quitFlag = 1;
        }

    }

	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client2.removeTuioListener(this);

		client2.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o) {
		lock(objectList) {
			objectList.Add(o.SessionID, o);
			initialOrientations.Add(o.SessionID, o.Angle); // Store the initial orientation
			}
		//lock(objectList) {
		//	objectList.Add(o.SessionID,o);
		//} if (verbose) Console.WriteLine("add obj "+o.SymbolID+" ("+o.SessionID+") "+o.X+" "+o.Y+" "+o.Angle);
	}

	public void updateTuioObject(TuioObject o) {

		if (verbose) Console.WriteLine("set obj "+o.SymbolID+" "+o.SessionID+" "+o.X+" "+o.Y+" "+o.Angle+" "+o.MotionSpeed+" "+o.RotationSpeed+" "+o.MotionAccel+" "+o.RotationAccel);
		// Calculate the change in orientation
		float currentOrientationDegrees = (float)(o.Angle * (180.0 / Math.PI));
		currentOrientationDegrees = currentOrientationDegrees % 360; // Ensure it's between 0 and 359

		// Determine the color based on the orientation range
		if (currentOrientationDegrees >= 0 && currentOrientationDegrees < 90) {
			objBrush = new SolidBrush(Color.Blue);
			menu = Image.FromFile(pizza);
			menu2 = Image.FromFile(pancake);
			flag2 = 1;
		} else if (currentOrientationDegrees >= 90 && currentOrientationDegrees < 180) {
			objBrush = new SolidBrush(Color.Red);
			menu = Image.FromFile(fries);
			menu2 = Image.FromFile(toast);
			flag2 = 1;
		} else if (currentOrientationDegrees >= 180 && currentOrientationDegrees < 270) {
			objBrush = new SolidBrush(Color.Green);
			menu = Image.FromFile(flafel);
			menu2 = Image.FromFile(omelette);
			flag2 = 1;
		} else { // 270 to 359
			objBrush = new SolidBrush(Color.White);
			menu = Image.FromFile(steak);
			menu2 = Image.FromFile(flafel);
			flag2 = 1;	
		}
	}

	public void removeTuioObject(TuioObject o) {

		lock (objectList)
		{
			objectList.Remove(o.SessionID);
			initialOrientations.Remove(o.SessionID); // Remove the initial orientation
		}
    //lock(objectList) {
    //	objectList.Remove(o.SessionID);
    //}
    //if (verbose) Console.WriteLine("del obj "+o.SymbolID+" ("+o.SessionID+")");
}

	public void addTuioCursor(TuioCursor c) {
		lock(cursorList) {
			cursorList.Add(c.SessionID,c);
		}
		if (verbose) Console.WriteLine("add cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y);
	}

	public void updateTuioCursor(TuioCursor c) {
		if (verbose) Console.WriteLine("set cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y+" "+c.MotionSpeed+" "+c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c) {
		lock(cursorList) {
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur "+c.CursorID + " ("+c.SessionID+")");
 	}

	public void addTuioBlob(TuioBlob b) {
		lock(blobList) {
			blobList.Add(b.SessionID,b);
		}
		if (verbose) Console.WriteLine("add blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area);
	}

	public void updateTuioBlob(TuioBlob b) {
		
		if (verbose) Console.WriteLine("set blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area+" "+b.MotionSpeed+" "+b.RotationSpeed+" "+b.MotionAccel+" "+b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b) {
		lock(blobList) {
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb "+b.BlobID + " ("+b.SessionID+")");
	}
	
	public void refresh(TuioTime frameTime) {
		Invalidate();
	}
		
    protected override void OnPaintBackground(PaintEventArgs pevent)
	{
			
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(bgrBrush, new Rectangle(0,0,width,height));
		Image backgroundImage = Image.FromFile(imagePath);
		Image plateimage = Image.FromFile(plate);
		Image blackPlateimage = Image.FromFile(blackPlate);

		Image omeletteimage = Image.FromFile(omelette); 
		Image toastimage = Image.FromFile(toast); 
		Image pancakeimage = Image.FromFile(pancake); 

		Image pastaimage = Image.FromFile(pasta); 
		Image steakimage = Image.FromFile(steak); 
		Image pizzaimage = Image.FromFile(pizza); 
			
		Image flafelimage = Image.FromFile(flafel); 
		Image friesimage = Image.FromFile(fries); 
		Image cakeimage = Image.FromFile(cake); 
			
		Image forkimage = Image.FromFile(fork);
		Image knifeimage = Image.FromFile(knife);

		if (flag2 == 0)
		{
			menu = Image.FromFile(pasta);
			menu2 = Image.FromFile(cake);
		}
			

		// Table & plaates
        g.DrawImage(backgroundImage, new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height));
		g.DrawImage(plateimage, new Rectangle((this.ClientSize.Width/2)-120, (this.ClientSize.Height / 2)-70, 350, 350));

		g.DrawImage(knifeimage, new Rectangle((this.ClientSize.Width/2)+100, (this.ClientSize.Height / 2)-60, 300, 300));
		g.DrawImage(forkimage, new Rectangle((this.ClientSize.Width/2)-300, (this.ClientSize.Height / 2)-60, 300, 300));

		g.DrawImage(plateimage, new Rectangle((this.ClientSize.Width/2)-800, (this.ClientSize.Height / 2)-170, 350, 350));
		g.DrawImage(blackPlateimage, new Rectangle((this.ClientSize.Width/2)+450, (this.ClientSize.Height / 2)-280, 350, 350));
		
		g.DrawImage(menu, new Rectangle((this.ClientSize.Width / 2) - 800, (this.ClientSize.Height / 2) - 170, 350, 350));
		g.DrawImage(menu2, new Rectangle((this.ClientSize.Width / 2) - 60, (this.ClientSize.Height / 2) - 40, 250, 250));
    // Recommendation Plates

    // breakfast
    if (currentHour >= 9 && currentHour < 12)
	{
        if (flag == 0)
        {
            g.DrawImage(omeletteimage, new Rectangle((this.ClientSize.Width / 2) + 450, (this.ClientSize.Height / 2) - 280, 350, 350));
        }
        if (flag == 2)
        {
            g.DrawImage(toastimage, new Rectangle((this.ClientSize.Width / 2) + 530, (this.ClientSize.Height / 2) - 220, 200, 200));
        }
        if (flag == 4)
        {
            g.DrawImage(pancakeimage, new Rectangle((this.ClientSize.Width / 2) + 530, (this.ClientSize.Height / 2) - 220, 200, 200));
        }
    }
	if (currentHour >= 12 && currentHour < 18)
	{
        if (flag == 0)
        {
            g.DrawImage(pastaimage, new Rectangle((this.ClientSize.Width / 2) + 450, (this.ClientSize.Height / 2) - 280, 350, 350));
        }
        if (flag == 2)
        {
            g.DrawImage(steakimage, new Rectangle((this.ClientSize.Width / 2) + 510, (this.ClientSize.Height / 2) - 260, 280, 280));
        }
        if (flag == 4)
        {
            g.DrawImage(pizzaimage, new Rectangle((this.ClientSize.Width / 2) + 500, (this.ClientSize.Height / 2) - 230, 250, 250));
        }
    }
	if(currentHour >= 18 && currentHour < 23)
	{
        if (flag == 0)
        {
            g.DrawImage(flafelimage, new Rectangle((this.ClientSize.Width / 2) + 450, (this.ClientSize.Height / 2) - 280, 320, 320));
        }
        if (flag == 2)
        {
            g.DrawImage(friesimage, new Rectangle((this.ClientSize.Width / 2) + 450, (this.ClientSize.Height / 2) - 280, 320, 320));
        }
        if (flag == 4)
        {
            g.DrawImage(cakeimage, new Rectangle((this.ClientSize.Width / 2) + 500, (this.ClientSize.Height / 2) - 220, 250, 250));
        }
    }
			
		//g.DrawImage(omeletteimage, new Rectangle((this.ClientSize.Width/2)+450, (this.ClientSize.Height / 2)-280, 350, 350));
		//g.DrawImage(toastimage, new Rectangle((this.ClientSize.Width/2)+530, (this.ClientSize.Height / 2)-220, 200, 200));
		//g.DrawImage(pancakeimage, new Rectangle((this.ClientSize.Width/2)+530, (this.ClientSize.Height / 2)-220, 200, 200));

		// Launch
		//g.DrawImage(pastaimage, new Rectangle((this.ClientSize.Width/2)+450, (this.ClientSize.Height / 2)-280, 350, 350));
		//g.DrawImage(steakimage, new Rectangle((this.ClientSize.Width/2)+510, (this.ClientSize.Height / 2)-260, 280, 280));
		//g.DrawImage(pizzaimage, new Rectangle((this.ClientSize.Width/2)+500, (this.ClientSize.Height / 2)-230, 250, 250));

		// Dinner 
		//g.DrawImage(flafelimage, new Rectangle((this.ClientSize.Width/2)+450, (this.ClientSize.Height / 2)-280, 320, 320));
		//g.DrawImage(friesimage, new Rectangle((this.ClientSize.Width/2)+450, (this.ClientSize.Height / 2)-280, 320, 320));
		//g.DrawImage(cakeimage, new Rectangle((this.ClientSize.Width / 2) + 500, (this.ClientSize.Height / 2) - 220, 250, 250));



    // draw the cursor path
    if (cursorList.Count > 0) {
 			lock(cursorList) {
			foreach (TuioCursor tcur in cursorList.Values) {
				List<TuioPoint> path = tcur.Path;
				TuioPoint current_point = path[0];

				for (int i = 0; i < path.Count; i++) {
					TuioPoint next_point = path[i];
					g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
					current_point = next_point;
				}
				g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
				g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
			}
		}
		}

    // draw the objects
		if (objectList.Count > 0)
		{
			lock (objectList)
			{
				foreach (TuioObject tobj in objectList.Values)
				{
					int ox = tobj.getScreenX(width);
					int oy = tobj.getScreenY(height);
					int size = height / 10;

					g.TranslateTransform(ox, oy);
					g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
					g.TranslateTransform(-ox, -oy);

					g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

					g.TranslateTransform(ox, oy);
					g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
					g.TranslateTransform(-ox, -oy);


					for(int i = 0; i < 10; i++)
					{
						if (i == tobj.SymbolID)
						{
							symbolIndex = i;
						}	
					}
					// Check if the symbol ID is 22, and if so, display "Waiter"
					//string symbolText = tobj.SymbolID == symbolIndex ? "ID: " + users[symbolIndex].Id + "\n" +"Name: " + users[symbolIndex].Name +"\n"+"Age: "+ users[symbolIndex].Age + "\n" + "Experience Years: "+ users[symbolIndex].YearsOfWorking : tobj.SymbolID.ToString();
					//g.DrawString(symbolText, font, fntBrush, new PointF(ox - 10, oy - 10));
				}
			}
		}
			
    // draw the blobs
    if (blobList.Count > 0) {
			lock(blobList) {
				foreach (TuioBlob tblb in blobList.Values) {
					int bx = tblb.getScreenX(width);
					int by = tblb.getScreenY(height);
					float bw = tblb.Width*width;
					float bh = tblb.Height*height;

					g.TranslateTransform(bx, by);
					g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
					g.TranslateTransform(-bx, -by);

					g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

					g.TranslateTransform(bx, by);
					g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
					g.TranslateTransform(-bx, -by);
						
					g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
				}
			}
		}
	}
	public async Task CallPythonServiceAsync()
{
    // Define the IP address and port number to listen on
    IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
    int port = 12345;

    // Create a TCP listener
    TcpListener listener = new TcpListener(ipAddress, port);

    // Start listening for incoming connections
    listener.Start();

    Console.WriteLine("Waiting for a connection...");

    try
    {
        // Accept the incoming connection asynchronously
        using (TcpClient client = await listener.AcceptTcpClientAsync())
        {
            Console.WriteLine("Connection accepted.");
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[1024];
                while (true)
                {
                    int bytes = await stream.ReadAsync(data, 0, data.Length);
                    string emotion = Encoding.ASCII.GetString(data, 0, bytes);

                    // Invoke on UI thread
                    this.Invoke((MethodInvoker)delegate
                    {
                        state.Text = "Received emotion: " + emotion;
                    });

                    if (quitFlag == 1)
                        break;
                }
            }
        }
    }
    finally
    {
        listener.Stop();
    }
}

	private void InitializeComponent()
	{
			this.SuspendLayout();
		// 
		// TuioDemo
		// 

		
			reco = new Label();
			reco.Text = "Recommendations";
			reco.Location = new Point((this.ClientSize.Width / 2) + 550, (this.ClientSize.Height / 2) - 380);
			reco.Size = new Size(300, 50);
			reco.Font = new Font("Arial", 20);
			reco.BackColor= Color.Transparent;
			this.Controls.Add(reco);
			tttime.Tick += Tttime_Tick;

			tttime.Start();
			tt.Tick += Tt_Tick;
			tt.Interval = 3000;
			tt.Start();
			this.WindowState = FormWindowState.Maximized;
			//this.ClientSize = new System.Drawing.Size(284, 261);
			this.Load += TuioDemo_Load2;
			this.Name = "MSTF";
			//this.Load += new System.EventHandler(this.TuioDemo_Load);
			this.ResumeLayout(false);
			this.KeyDown += TuioDemo_KeyDown;

			timeLabel = new Label();
			timeLabel.Location = new Point((this.ClientSize.Width / 2) + 100, 0); // Example position
			timeLabel.Size = new Size(200, 40); // Adjusted size for larger font
			timeLabel.Text = DateTime.Now.ToString("HH:mm:ss"); // Initial time display
			timeLabel.Font = new Font("Arial", 26, FontStyle.Bold); // Set font here
			timeLabel.BackColor = Color.Transparent;
			this.Controls.Add(timeLabel); 
	}

	private void Tttime_Tick(object sender, EventArgs e)
	{
		timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
	}

	private void TuioDemo_Load2(object sender, EventArgs e)
	{
		InitializeComponent();
	}
	int ct = 0;
	int flag = 0;
	private void Tt_Tick(object sender, EventArgs e)
	{
        
		flag++;
		if (flag == 6)
			flag = 0;
		ct++;

		//state.Text = "Customer State: SAD";

		Invalidate();

	}

	private void TuioDemo_Load1(object sender, EventArgs e)
	{
		
	}

	private void TuioDemo_KeyDown(object sender, KeyEventArgs e)
	{
        
		if (e.KeyData == Keys.F1)
		{
			if (fullscreen == false)
			{

				width = screen_width;
				height = screen_height;

				window_left = this.Left;
				window_top = this.Top;

				this.FormBorderStyle = FormBorderStyle.None;
				this.Left = 0;
				this.Top = 0;
				this.Width = screen_width;
				this.Height = screen_height;

				fullscreen = true;
			}
			else
			{

				width = window_width;
				height = window_height;

				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.Left = window_left;
				this.Top = window_top;
				this.Width = window_width;
				this.Height = window_height;

				fullscreen = false;
			}
		}
		else if (e.KeyData == Keys.Escape)
		{
			this.Close();

		}
		else if (e.KeyData == Keys.V)
		{
			verbose = !verbose;
		}
	}

	private void TuioDemo_Load(object sender, EventArgs e)
	{
        
	}

    

	public static void Main(String[] argv) {
	 		int port = 0;
			switch (argv.Length) {
				case 1:
					port = int.Parse(argv[0],null);
					if(port==0) goto default;
					break;
				case 0:
					port = 3333;
					break;
				default:
					Console.WriteLine("usage: mono TuioDemo [port]");
					System.Environment.Exit(0);
					break;
			}
			
			TuioDemo app = new TuioDemo(port);
			Application.Run(app);
		}
}
