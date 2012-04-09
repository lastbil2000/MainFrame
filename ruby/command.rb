require 'mscorlib'
#require 'curses'
require 'readline'
require 'System'
#require 'IronRuby.dll'
#unless Kernel.respond_to?(:require_relative)
#  module Kernel
#    def require_relative(path)
#      require File.join(File.dirname(caller[0]), path.to_str)
#    end
#  end
#end

#caller.each{ |cc| puts "HELLO: " + cc}

require '/home/olga/workspace/robot/MainFrame/MainFrame/bin/Debug/scripts/helpers/arm.rb'

#installerade ruby med --prefix=/usr
class MainClass

	def initialize (global_object)
		@robot = global_object.robot
		@should_run = false
	end
	def setup
		puts "Starting command!%!%!"
	end

	def loop		
		return false
	end

	def stop
		@should_run = false
	end

	def should_run
		return @should_run
	end
	
	def mediator_types
		return ["Speech.SphinxASRProcess+TextReceived"]
	end

	def request (messageType, message)
		if messageType == "Speech.SphinxASRProcess+TextReceived"
			msg = message.data.downcase
			#puts "HELLO WORLD: " + msg
			
			if (msg.split(" ").first == "arm")
				arm_command_helper msg,  @robot.get("arm")
			else
				speech_interpreter = @robot.get("speech_interpreter")
				speech_interpreter.try_interpret(msg)
			end
		end
		return msg
	end


end

self.main_class = MainClass.new(self)
self.mediator_types = self.main_class.mediator_types

