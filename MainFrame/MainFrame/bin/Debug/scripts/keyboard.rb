require 'mscorlib'
#require 'curses'
require 'readline'
require 'System'
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
		interpret line
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
		command_name = device_name = line.split(" ").first

		if (command_name == "restart")
			device_name = line.split(" ").last
			device = @go.robot.get(device_name)
			if (device != nil)
				puts "restarting: #{device_name}" 
				device.stop
				device.start
			else
				puts "no device named: #{device_name}"
			end

			return true
		elsif (command_name == "blabla")
			puts "YOU ARE WRITING SHIT"
			return true
		end

		return false
	end

	# Tries to fetch a device from the robot and execute a method specified
	def evaluate (line)
		device_name = line.split(".").first
		device = @go.robot.get(device_name)

		if (device != nil)
			command = line[device_name.length, line.length - device_name.length]
			puts eval("device" + command)
			return true
		end
		
		return false
	end

	def stop
		@should_run = false
	end

	def should_run
		return @should_run
	end


end

self.main_class = MainClass.new(self)

