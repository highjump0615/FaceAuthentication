"use strict";
var asset_url = 'assets'
function SoundEffect()
{
	this.computer1=null;
	this.robot_startup2=null;
	this.drumroll=null;
	this.roll_finish1=null;

    this.m_bIE      = false;
    this.m_bOn      = true;
	
	this.Init();
}

SoundEffect.prototype.Init = function()
{
    var aBody = document.getElementsByTagName('body');
    if (aBody.length <= 0)
        return;

    this.m_bOn = true;

    // if ("IE" === Common.Get_Browser())
        // this.m_bIE = true;

    var oBody = aBody[0];
    this.computer1 = this.private_AddSound(oBody, "famous-app-sound-computer1", asset_url+"/sounds/computer1.mp3");
    this.robot_startup2   = this.private_AddSound(oBody, "famous-app-sound-robot_startup2",   asset_url+"/sounds/robot-startup2.mp3");
    this.drumroll   = this.private_AddSound(oBody, "famous-app-sound-drumroll",   asset_url+"/sounds/drumroll.mp3");
    this.roll_finish1   = this.private_AddSound(oBody, "famous-app-sound-roll_finish1",   asset_url+"/sounds/roll-finish1.mp3");
};
SoundEffect.prototype.On = function()
{
    this.m_bOn = true;
};
SoundEffect.prototype.Off = function()
{
    this.m_bOn = false;
};
SoundEffect.prototype.private_AddSound = function(oBody, sId, sPath)
{
    var oElement = document.getElementById(sId);

    if (!oElement)
    {
        oElement = document.createElement("audio");
        oElement.id      = sId;
        oElement.preload = "preload";
        oElement.src     = sPath;
        oBody.appendChild(oElement);
    }

    return oElement;
};
SoundEffect.prototype.public_PlaySound = function(oAudio)
{
    if (!oAudio || !this.m_bOn)
        return;

    try
    {
        if (this.m_bIE)
        {
            oAudio.setActive();
            oAudio.click();
            oAudio.autoplay = "";
            oAudio.autoplay = "autoplay";
        }
        else
            oAudio.play();
    }
    catch(e)
    {
    }
};