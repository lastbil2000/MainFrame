require 'mscorlib'
#require 'readline'

class MainClass

	def initialize (global_object)
		@go = global_object
		@should_run = true
	end
	def setup
		puts "SETUP"
	end
	def loop
		
		line = gets.gsub("\n","").strip
		if (line == "exit")
			@go.robot.stop
		else
			device_name = line.split(".").first
			device = @go.robot.get(device_name)

			if (device == nil)
				puts "UNKNOWN COMMAND"
			else
				command = line[device_name.length, line.length - device_name.length]
				puts eval("device" + command)
			end
			
		end
	end

	def stop
		@should_run = false
	end

	def should_run
		return @should_run
	end


end

self.main_class = MainClass.new(self)

