<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		
		<html xmlns="http://www.w3.org/1999/xhtml">
			<head>
				<title>Start Page</title>
				<link href="mru.css" type="text/css" rel="stylesheet"/>
			</head>
			<body>
				<h2>
					<div style="width:100%; background-color:#eeeeee;padding:4px">
					<span class="x" style="margin-left:10px" >
						<img src="profiler_logo.png" alt="Profiler Logo" style="vertical-align:middle;margin-right:5px"/>
						IJW Profiler
					</span>
					<span class="y">
						<script>
							<![CDATA[document.write( window.external.GetVersion() );]]>
						</script>
					</span>
					</div>
				</h2>
				<div>
					<div style="width:100%; ">
						<a href="#" onclick="javascript:window.external.Run('','','')">Profile an application...</a>
						<br/>
						If you have profiled an application recently, you can select it from the list below:
					</div>
					<div style="margin-left:40px">
						<ul>
							<xsl:for-each select="/mru/run">
								<li>
									<a href="#">
										<xsl:attribute name="onclick">
											javascript:window.external.Run(
											'<xsl:value-of select="./cmd"/>',
											'<xsl:value-of select="./dir"/>',
											'<xsl:value-of select="./args"/>');
										</xsl:attribute>
										<xsl:value-of select="./cmd"/>
									</a>
								</li>
							</xsl:for-each>
						</ul>
					</div>
					<!--Code for previous run loading (todo)
					<div style="margin-top:30px">
						<a href="#" onclick="javascript:window.external.Snapshot('')">
							Load snapshot from a previous profiling run...
						</a>
						<br/>
						If you have viewed the snapshot recently, select it from the list below:
					</div>
					<div style="margin-left:40px">
						<ul>
							<xsl:for-each select="/mru/snapshot">
								<li>
									<a href="#">
										<xsl:attribute name="onclick">
											javascript:window.external.Snapshot('<xsl:value-of select="./path"/>');
										</xsl:attribute>
										<xsl:value-of select="./title"/>
									</a>
								</li>
							</xsl:for-each>
						</ul>
					</div>-->
					<div style="margin-top:30px">
						<a href="#" onclick="javascript:window.external.Update('')">
							Check for updates...
						</a>
						<br/>
						Check online for updates to the IJW Profiler
					</div>
				</div>
				<div class="footer">
					Other products from It Just Works Software that you may like:
					<br/>
					<img src="corfu_logo.png" style="border: 0px; vertical-align: middle;margin-right:0.5em"/>
					<a href="#" onclick="javascript:window.external.OpenExternalLink('http://www.ijw.co.nz/corfu.htm')">Corfu Text Editor
					</a>
				</div>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>
