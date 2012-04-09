def arm_command_helper (msg, arm)
	commands = msg.split(" ")
	if commands.length < 2
		return false
	end
	speed = 10.0

	if commands.length == 3 && commands.at(2) == "ten"
		speed = speed * 2.0
	end

	if (commands.at(1) == "up")
		arm.up += speed
	elsif (commands.at(1) == "down")
		arm.up -= speed
	elsif (commands.at(1) == "right")
		arm.rotation += speed
	elsif (commands.at(1) == "left")
		arm.rotation -= speed
	elsif (commands.at(1) == "open")
		arm.open = true
	elsif (commands.at(1) == "close")
		arm.open = false
	end


	return true
end

def blah
	puts "BLAH"
end
