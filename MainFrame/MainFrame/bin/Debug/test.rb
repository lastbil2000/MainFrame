#require 'mscorlib'
require 'curses'
#require 'readline'

class MainClass
	def initialize (global_object)
		@go = global_object
	end
	def setup
		puts "SETUP"
	end
	def loop
		@go.robot.get("dummy").value +=1
		puts @go.robot.get("dummy").value
		#puts @go.robot.get("dummy")
		STDOUT.sync = true
		line = gets.chomp

		if line != nil && line.length > 0
			puts "PYTSAN #{line}" 
		end
	end
	def should_run
		true
	end

end

#self.main_class = MainClass.new(self)

