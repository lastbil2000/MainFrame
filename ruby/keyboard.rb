require 'mscorlib'
#require 'curses'
require 'readline'
require 'System'
#require 'IronRuby.dll'

#installerade ruby med --prefix=/usr
class MainClass

	def initialize (global_object)
		@go = global_object
		@should_run = true
		@line = ""
	end
	def setup
		
		puts "SETUP"
		#Curses.setpos(0,0)
	end

	def loop_old
		
		char = IO.binread("/dev/stdin", 1)
		if (char != "\n")
			if (/[a-z0-9\ \,\.\=\_\"\']/.match(char) != nil)
				putc char
				@line += char
			end
		elsif (char == "\n")
			if (interpret @line) == false
				puts "Unknown command: " + @line
			end
			@line = ""
		end

	end

	def loop

		line = System::Console.read_line

		if line.length > 0
			if (interpret line) == false
				puts "Unknown command: " + line
			end
		end
	end

	def interpret (line)
		
		if (line == "exit")
			@go.robot.stop
			return true
		elsif (command line)
			return true
		elsif (evaluate line)
			return true
		end
		
		return false
	end

	# Execptues a custom, static command:
	def command (line)
		command_name = line.split(" ").first

		
		if (command_name == "restart" || 
		    command_name == "start" || 
			command_name == "stop")
			device_name = line.split(" ").last
			device = @go.robot.get(device_name)
			if (device != nil)
				if (command_name == "restart")
					puts "restarting: #{device_name}" 
					device.stop
					device.start
				elsif (command_name == "start")
					puts "starting: #{device_name}"
					@go.robot.start_device (device_name)
				elsif (command_name == "stop")
					puts "stopping: #{device_name}"
					@go.robot.stop_device (device_name)
				end
			else
				puts "no device named: #{device_name}"
			end

			return true
		elsif (command_name == "looking")
			#factory = @go.robot.get("device_factory")
			#camera = @go.robot.get("video0")
			#face_obj = factory.GetHaarCapture(camera, "haarcascade_frontalface_alt.xml")
			#face_obj.start
			#@go.robot.add_device (
			#	"looking",
			#	@go.robot.get("process_factory").GetLookAtPeopleProcess (
			#		face_obj,
			#		@go.robot.get("head")
			#	)
			#)
			
			return true
		elsif (command_name == "rload")
			device_name = line.split(" ").last
			factory = @go.robot.get("process_factory")
			#rubyp = 
			puts "Starting device: #{device_name}"
			@go.robot.add_device(device_name, factory.get_ruby("scripts/" + device_name + ".rb"))
			@go.robot.start_device (device_name)
			return true
			
		end

		return false
	end

	# Tries to fetch a device from the robot and execute a method specified
	def evaluate (line)
		device_name = line.split(".").first
		device = @go.robot.get(device_name)
		begin

			if (device != nil)
				command = line[device_name.length, line.length - device_name.length]
				puts eval("device" + command)
				return true
			end


			rescue System::MissingMethodException
				puts "MISSING METHOD ON DEVICE: " + device_name
			rescue System::Exception
				puts "UNABLE TO INTERPRET: " + line
		end
		return false
	end

	def stop
		@should_run = false
	end

	def should_run
		return @should_run
	end
	
	def mediator_types
		return []
	end

	def request (messageType, message)
		if messageType == "Speech.SphinxASRProcess+TextReceived"
			puts "I got this message: " + message.data
		end
	end


end

self.main_class = MainClass.new(self)
self.mediator_types = self.main_class.mediator_types

