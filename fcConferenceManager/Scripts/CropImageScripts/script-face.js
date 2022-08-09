$(document).ready(function(){
	$("#file").change(function(e){
		var img = e.target.files[0];

		if(!pixelarity.open(img, false, function(res, faces){			
			console.log(faces);

			$("#result").attr("src", res);
			$(".face").remove();

			for(var i = 0; i < faces.length; i++){				
				$("body").append("<div class='face' style='height: "+faces[i].height+"px; width: "+faces[i].width+"px; top: "+($("#result").offset().top + faces[i].y)+"px; left: "+($("#result").offset().left + faces[i].x)+"px;'>");
			}
            var img = new Image();
            img.onload = function () {
                canvas.height = height;
                canvas.width = width;
                context.drawImage(img, x1, y1, width, height, 0, 0, width, height);

                $('#imgCropped').val(canvas.toDataURL());

                $('#canvas').show();

                $('#result').hide();
                ;
                $('#btnCrop').hide();

            };
            JcropAPI = $('#result').data('Jcrop');
            if (JcropAPI != null) {
                JcropAPI.destroy();
            }
            img.src = $('#result').attr("src");
            $('#lbl1').text("Cropped Image Saved");
			
		}, "jpg", 0.7, true)){
			alert("Whoops! That is not an image!");
		}
        

	});
});