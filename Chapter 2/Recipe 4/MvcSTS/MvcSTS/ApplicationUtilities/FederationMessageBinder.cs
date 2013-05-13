using System;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;

public class WSFederationMessageModelBinder : IModelBinder
{
    public object BindModel(ControllerContext controllerCtx, ModelBindingContext bindingCtx)
    {
        if (controllerCtx == null || bindingCtx == null)
        {
            throw new ArgumentException("Invalid Context");
        }

        try
        {
            var message = WSFederationMessage.CreateFromUri(controllerCtx.HttpContext.Request.Url);
            if (bindingCtx.ModelType.IsAssignableFrom(message.GetType()))
                return message;
        }
        catch (Exception) { }
        
        return null;
    }
}